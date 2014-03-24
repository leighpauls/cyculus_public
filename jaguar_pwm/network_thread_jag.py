import threading, socket, time

UDP_IP = "127.0.0.1"
UDP_PORT = 5678

# 200 tick encoder on a 4x counter behind a 5.5x reduction to the bike
ENCODER_TICKS_PER_DEGREE = -200.0 / 360.0 * 4 * 5.5

class NetworkThread(threading.Thread):
    """Handles input from the game and sets tells the serial thread what
    position commands to send"""
    def __init__(self, serial):
        threading.Thread.__init__(self)
        self.done = False
        self.done_signal = threading.Condition()
        self.enabled = False
        self.serial = serial
        self.start()

    def run(self):
        self.done_signal.acquire()

        try:
            print "opening control mode socket..."
            sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
            sock.bind((UDP_IP, UDP_PORT))
            sock.settimeout(1.0)

            while not self.done:
                if not self.enabled:
                    time.sleep(0.01)
                    continue

                try:
                    data = sock.recv(1024)
                except socket.timeout:
                    continue

                words = data.split()
                if len(words) == 0:
                    continue
                if words[0] == 'pitchDeg' and len(words) == 2:
                    # convert the angle to encoder ticks
                    angleDeg = float(words[1])
                    ticks = int(ENCODER_TICKS_PER_DEGREE * angleDeg)
                    if self.enabled:
                        self.serial.set_new_setpoint(ticks)
                else:
                    print "invalid message: ", data
        finally:
            self.done_signal.release()

    def enable(self):
        self.enabled = True
    def disable(self):
        self.enabled = False
    def kill(self):
        print "trying to kill network..."
        self.done = True
        self.done_signal.acquire()
        print "done killing serial"
