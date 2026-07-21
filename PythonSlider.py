from tkinter import *
import socket

UDP_IP = "127.0.0.1"
UDP_PORT = 8005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

root = Tk()
root.title("TKinter Slider")
root.geometry("400x200")

value = 0
is_on = True

def send_data_to_unity():
    """Helper function to grab current states, bundle them, and send via UDP"""
    # 1. Get the current slider value
    current_value = float(slider.get()) / 100
    

    state_int = 1 if is_on else 0
    
    # 3. Format as a comma-separated string (e.g., "45.5,1")
    message = f"{current_value},{state_int}".encode('utf-8')
    
    try:
        sock.sendto(message, (UDP_IP, UDP_PORT))
    except Exception as e:
        print(f"Failed to send data: {e}")

def toggle():
    global is_on
    is_on = not is_on
    if is_on:
        toggle_btn.config(text='ON', bg='green', fg='white')
        status_label.config(text='Status: Active')
    else:
        toggle_btn.config(text='OFF', bg='red', fg='white')
        status_label.config(text='Status: Inactive')
    send_data_to_unity()

def update_label(value):
    value_label.config(text=f"Value: {value}")
    send_data_to_unity() 

slider = Scale(root, from_=0.00, to=100, orient=HORIZONTAL, length= 150, command= update_label)
slider.pack(padx=20, side=LEFT)
slider.place(x=120, y=150)


toggle_btn = Button(root, text='ON', bg='green', fg='white', font=('Arial', 16), command=toggle, width=10)
toggle_btn.pack(pady=20)
toggle_btn.place(x=130, y=100)

status_label = Label(root, text='Status: Active', font=('Arial', 12))
status_label.pack()

value_label = Label(root, text="Value: 0")
value_label.pack()

root.mainloop()