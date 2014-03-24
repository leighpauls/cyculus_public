import threading, serial, time

BAUD_RATE = 115200
SERIAL_READ_TIMEOUT = 0.05
MESSAGE_ACK_TIMEOUT = 0.1
MESSAGE_QUEUE_HIGH_WATERMARK = 5

class JagSerialComm(threading.Thread):
    """Thread for communicating with the pitch control arduino over serial"""
    def __init__(self, tty):
        threading.Thread.__init__(self)
        self.serial_port = serial.Serial(
            tty,
            BAUD_RATE,
            timeout=SERIAL_READ_TIMEOUT)
        self.done = False
        self.done_signal = threading.Condition()
        self.output_queue_signal = threading.Condition()
        self.output_queue = []
        self.pending_ack = None
        self.cur_message_num = 0
        self.ready = False;
        self.start()

    def run(self):
        """ The serial port RX thread """
        self.done_signal.acquire()
        try:
            while not self.done:
                data = self.serial_port.readline()
                words = data.split()
                if len(words) != 0:
                    self._handle_input(words)
                # see if the current pending ack has timed out
                self._check_pending_ack()
        finally:
            self.serial_port.close()
            self.done_signal.release()

    def _handle_input(self, words):
        if words[0] == 'ack':
            if len(words) != 2 or int(words[1]) != self.cur_message_num:
                print "invalid ack"
                return
            # clear the last ack
            self.pending_ack = None
            self.output_queue_signal.acquire()
            self.output_queue.pop(0)
            self.output_queue_signal.release()
            # try to send the next message
            self._try_sending_output()
        elif words[0] == 'NACK':
            print "nacked"
            # immediately resend the last message
            self.pending_ack = None
            self._try_sending_output()
        elif words[0] == 'jagReady':
            print "Recieved ready message"
            self.ready = True
        else:
            print "Badly formed message: ", " ".join(words)

    def _check_pending_ack(self):
        if self.pending_ack is None or self.pending_ack > time.time():
            return
        # the ack has timed out
        print "timeout"
        self.pending_ack = None
        self._try_sending_output()

    def _try_sending_output(self):
        self.output_queue_signal.acquire()
        if self.pending_ack is not None or len(self.output_queue) == 0:
            self.output_queue_signal.release()
            return

        # send the next message
        self.cur_message_num = (self.cur_message_num + 1) % 10000
        self.pending_ack = time.time() + MESSAGE_ACK_TIMEOUT
        self.serial_port.write(
            self.output_queue[0] + " " + str(self.cur_message_num) + '\n')

        self.output_queue_signal.release()

    def set_new_setpoint(self, setpoint):
        self.output_queue_signal.acquire()
        self.output_queue.append("set " + str(setpoint) + " " + str(setpoint))
        if len(self.output_queue) > MESSAGE_QUEUE_HIGH_WATERMARK:
            print "output queue past high water mark: ", len(self.output_queue)
        self.output_queue_signal.release()
        self._try_sending_output()

    def kill(self):
        print "trying to kill serial..."
        self.done = True
        self.done_signal.acquire()
        print "done killing serial"

    def feedUpCommand(self):
        self.serial_port.write("fu\n")
    def feedDownCommand(self):
        self.serial_port.write("fd\n")
    def stopCommand(self):
        self.serial_port.write("s\n")
    def controlCommand(self):
        self.serial_port.write("control\n")
