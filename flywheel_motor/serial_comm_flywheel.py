import threading, serial, time, socket

BAUD_RATE = 115200
SERIAL_READ_TIMEOUT = 0.05
MESSAGE_ACK_TIMEOUT = 0.1
MESSAGE_QUEUE_HIGH_WATERMARK = 5

UDP_IP = "127.0.0.1"
UDP_PORT = 5680

class FlywheelSerialComm(threading.Thread):
    """Thread for communicating with the flywheel control arduino over serial"""
    def __init__(self, tty):
        threading.Thread.__init__(self)
        self.serial_port = serial.Serial(
            tty,
            BAUD_RATE,
            timeout=SERIAL_READ_TIMEOUT)
        self.done = False
        self.done_signal = threading.Condition()
        self.ready = False
        self.output_sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.start()

    def run(self):
        """The serial port RX thread"""
        self.done_signal.acquire()
        try:
            while not self.done:
                data = self.serial_port.readline()
                words = data.split()
                if len(words) != 0:
                    self._handle_input(words)
        finally:
            self.serial_port.close()
            self.done_signal.release()

    def _handle_input(self, words):
        if words[0] == 'controls':
            if len(words) != 3:
                print "invalid controls command:", ' '.join(words)
                return
            # read the new controls
            handlebar_pos_degrees = float(words[1])
            flywheel_speed_meters_per_sec = float(words[2])
            # send the controls on to the game
            message = "controls " + str(handlebar_pos_degrees) + " " \
                + str(flywheel_speed_meters_per_sec)
            self.output_sock.sendto(message, (UDP_IP, UDP_PORT))

        elif words[0] == 'flywheelReady':
            print "Recieved ready message"
            self.ready = True
        else:
            print "Badly formed message:", " ".join(words)

    def kill(self):
        print "trying to kill..."
        self.done = True
        self.done_signal.acquire()
        print "done killing"

    def set_pedal_force(self, force_newtons):
        self.serial_port.write("f " + str(force_newtons) + "\n")
