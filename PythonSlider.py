from tkinter import *
import socket

UDP_IP = "127.0.0.1"
UDP_PORT = 8005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

root = Tk()
root.title("TKinter Slider")
root.geometry("400x400")

value = 0
is_on = True
fov_is_on = True
resx = 1080
resy = 1080



def send_data_to_unity():
    """Helper function to grab current states, bundle them, and send via UDP"""
    # 1. Get the current slider value
    current_value = float(slider.get()) / 100
    render_scale = float(renderslider.get()) / 10
    try:
        resx = float(ResoXbox.get())
    except ValueError:
        resx= 1080
    try:
        resy = float(ResoYbox.get())
    except ValueError:
        resy= 1080
    state_int = 1 if is_on else 0
    fov_int = 1 if fov_is_on else 0
    
    # 3. Format as a comma-separated string (e.g., "45.5,1")
    message = f"{current_value},{state_int},{fov_int},{render_scale},{resx},{resy}".encode('utf-8')
    
    try:
        sock.sendto(message, (UDP_IP, UDP_PORT))
    except Exception as e:
        print(f"Failed to send data: {e}")

def toggle():
    global is_on
    is_on = not is_on
    if is_on:
        toggle_btn.config(text='ON', bg='green', fg='white')
    else:
        toggle_btn.config(text='OFF', bg='red', fg='white')
    send_data_to_unity()

def toggle_fov():
    global fov_is_on
    fov_is_on = not fov_is_on
    if fov_is_on:
        foveate_btn.config(text='ON', bg='green', fg='white')
    else:
        foveate_btn.config(text='OFF', bg='red', fg='white')
    send_data_to_unity()


def update_unity(value):
    send_data_to_unity() 

"FOVEATED RENDERING"

slider = Scale(root, from_=0.00, to=100, orient=HORIZONTAL, length= 150, command= update_unity)
slider.pack(padx=20, side=LEFT)
slider.place(x=5, y=90)

foveate_btn = Button(root, text='ON', bg='green', fg='white', font=('Arial', 16), command=toggle_fov, width=10)
foveate_btn.pack(pady=30)
foveate_btn.place(x=10, y=50)

foveate_label = Label(root, text='Foveated Rendering', font=('Arial', 12) )
foveate_label.pack()
foveate_label.place(x=12, y=20)

"GAZE TRACKING"

toggle_btn = Button(root, text='ON', bg='green', fg='white', font=('Arial', 16), command=toggle, width=10)
toggle_btn.pack(pady=20)
toggle_btn.place(x=10, y=180)

status_label = Label(root, text='Gaze tracking', font=('Arial', 12))
status_label.pack()
status_label.place(x = 12, y=150)

"RENDER SCALING"

renderslider = Scale(root, from_=1, to=20, orient=HORIZONTAL, length= 150, command= update_unity)
renderslider.pack(padx=20, side=LEFT)
renderslider.place(x=5, y=250)
renderlabel = Label(root, text='Render Scaling', font=('Arial', 12))
renderlabel.pack()
renderlabel.place(x = 12, y=230)

"RESOLUTION"
resolabel = Label(root, text='Resolution', font=('Arial', 12))
resolabel.pack()
resolabel.place(x = 12, y=300)
ResoXbox = Spinbox(root, from_=0, increment=100, width=5)
ResoXbox.pack()
ResoXbox.place(x=10, y=320)
ResoYbox = Spinbox(root, from_=0, increment=100, width=5)
ResoYbox.pack()
ResoYbox.place(x=60, y=320)
Resobtn = Button(root, text="Update", command=send_data_to_unity)
Resobtn.pack()
Resobtn.place(x=30, y=350)


root.mainloop()