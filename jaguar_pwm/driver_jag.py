#!/usr/bin/env python

import time, signal, sys
import socket

import serial_comm_jag, network_thread_jag

def run_jag_pwm_driver(tty):
    serial = serial_comm_jag.JagSerialComm(tty)
    networkThread = network_thread_jag.NetworkThread(serial)
    def signal_handler(signum, frame):
        serial.kill()
        networkThread.kill()
        sys.exit()

    signal.signal(signal.SIGINT, signal_handler)

    # wait for the driver to be ready
    while not serial.ready:
        time.sleep(0.01)

    print "use 's', 'fd', or 'control':"
    while True:
        print "> ",
        line = sys.stdin.readline()[:-1]
        if line == "fd":
            print "feeding down!"
            serial.feedDownCommand()
        elif line == "control":
            serial.controlCommand()
            networkThread.enable()
            print "controlling..."
        else:
            serial.stopCommand()
            networkThread.disable()
            if line != "s":
                print "unknown command: ", line
                print "use 's', 'fu', 'fd', or 'control'"
                print "stopped for safety"
            else:
                print "stopped"


if __name__ == '__main__':
    if len(sys.argv) != 2:
        print "Usage: ", sys.argv[0], " tty_address"
        sys.exit(1)
    run_jag_pwm_driver(sys.argv[1])
