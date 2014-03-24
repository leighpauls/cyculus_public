#!/usr/bin/env python

import time, signal, sys
import socket

import serial_comm_flywheel

UDP_IP = "127.0.0.1"
UDP_PORT = 5679

def run_flywheel_driver(tty):
    serial = serial_comm_flywheel.FlywheelSerialComm(tty)
    def signal_handler(signum, frame):
        serial.kill()
        sys.exit()
    signal.signal(signal.SIGINT, signal_handler)

    # wait for the driver to be ready
    while not serial.ready:
        time.sleep(0.01)

    print "opening the flywheel control socket..."

    listen_sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    listen_sock.bind((UDP_IP, UDP_PORT))

    while True:
        data = listen_sock.recv(1024)
        words = data.split()
        if len(words) == 0:
            continue
        if words[0] == 'pedalForce' and len(words) == 2:
            force_newtons = float(words[1])
            serial.set_pedal_force(force_newtons)
        else:
            print 'invalid message:', data

if __name__=='__main__':
    if len(sys.argv) != 2:
        print "Usage: ", sys.argv[0], ' tty_address'
        sys.exit(1)
    run_flywheel_driver(sys.argv[1])
