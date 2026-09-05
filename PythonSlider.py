from tkinter import *
import socket

UDP_IP = "127.0.0.1"
UDP_PORT = 8005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

root = Tk()
root.title("TKinter Slider")
root.geometry("900x400")

value = 0
is_on = True
fov_is_on = True
bloom_is_on = True
fog_is_on = False
ssao_is_on = True
resx = 1080
resy = 1080
antialias_on = False
vsync_on = False
motion_on = False

def send_udp_message(message):
    try:
        encoded_message = message.encode('utf-8')
        sock.sendto(encoded_message, (UDP_IP, UDP_PORT))
    except Exception as e:
        print(f"Failed to send data: {e}")

"FOVEATED RENDERING"

def update_foveation(dump):
    current_value = float(slider.get()) / 100
    fov_int = 1 if fov_is_on else 0
    message = f"0, {fov_int}, {current_value}"
    send_udp_message(message)

def toggle_fov():
    global fov_is_on
    fov_is_on = not fov_is_on
    if fov_is_on:
        foveate_btn.config(text='ON', bg='green', fg='white')
    else:
        foveate_btn.config(text='OFF', bg='red', fg='white')
    update_foveation(1)

slider = Scale(root, from_=0.00, to=100, orient=HORIZONTAL, length= 150, command= update_foveation)
slider.place(x=5, y=90)
foveate_btn = Button(root, text='ON', bg='green', fg='white', font=('Arial', 16), command=toggle_fov, width=10)
foveate_btn.place(x=10, y=50)
foveate_label = Label(root, text='Foveated Rendering', font=('Arial', 12) )
foveate_label.place(x=12, y=20)

"GAZE TRACKING"

def toggle():
    global is_on
    is_on = not is_on
    if is_on:
        toggle_btn.config(text='ON', bg='green', fg='white')
    else:
        toggle_btn.config(text='OFF', bg='red', fg='white')
    state_int = 1 if is_on else 0
    message = f"1, {state_int}"
    send_udp_message(message)

toggle_btn = Button(root, text='ON', bg='green', fg='white', font=('Arial', 16), command=toggle, width=10)
toggle_btn.place(x=10, y=180)
status_label = Label(root, text='Gaze tracking', font=('Arial', 12))
status_label.place(x = 12, y=150)

"RENDER SCALING"

def update_render(dump):
    render_scale = float(renderslider.get()) / 10
    message = f"2, {render_scale}"
    send_udp_message(message)

renderslider = Scale(root, from_=1, to=20, orient=HORIZONTAL, length= 150, command= update_render)
renderslider.place(x=5, y=250)
renderlabel = Label(root, text='Render Scaling', font=('Arial', 12))
renderlabel.place(x = 12, y=230)

"RESOLUTION"

def update_resolution():
    print("button")
    try:
        resx = float(ResoXbox.get())
    except ValueError:
        resx= 1080
    try:
        resy = float(ResoYbox.get())
    except ValueError:
        resy= 1080
    message = f"3, {resx}, {resy}"
    send_udp_message(message)

resolabel = Label(root, text='Resolution', font=('Arial', 12))
resolabel.place(x = 12, y=300)
ResoXbox = Spinbox(root, from_=0, increment=100, width=5)
ResoXbox.place(x=10, y=320)
ResoYbox = Spinbox(root, from_=0, increment=100, width=5)
ResoYbox.place(x=60, y=320)
Resobtn = Button(root, text="Update", command=update_resolution)
Resobtn.place(x=30, y=350)

"ANTI-ALIASING"

def update_aliasing():
    antialias = 1 if antialias_on else 0
    aliasvalue = antialiasbox.get()
    message = f"4, {antialias}, {aliasvalue}"
    send_udp_message(message)

def toggle_antialias():
    global antialias_on
    antialias_on = not antialias_on
    if antialias_on:
        antialias_btn.config(text='ON', bg='green', fg='white')
    else:
        antialias_btn.config(text='OFF', bg='red', fg='white')
    update_aliasing()

antialias_btn = Button(root, text='OFF', bg='red', fg='white', font=('Arial', 16), command=toggle_antialias, width=10)
antialias_btn.place(x=200, y=50)
antialias_label = Label(root, text='Anti Aliasing', font=('Arial', 12))
antialias_label.place(x = 212, y=20)
antialiasbox = Spinbox(root, from_=2, to=8, increment=2, width=5)
antialiasbox.place(x=200, y=110)
aliasbtn = Button(root, text="Update", command=update_aliasing)
aliasbtn.place(x=280, y=105)

"V-SYNC"

def updatevsync():
    global vsync_on
    vsync_on = not vsync_on
    if vsync_on:
        vsync_btn.config(text='ON', bg='green', fg='white')
    else:
        vsync_btn.config(text='OFF', bg='red', fg='white')
    vsync_int = 1 if vsync_on else 0
    message = f"5, {vsync_int}"
    send_udp_message(message)

vsync_btn = Button(root, text='OFF', bg='red', fg='white', font=('Arial', 16), command=updatevsync, width=10)
vsync_btn.place(x=200, y=180)
vsync_label = Label(root, text='V-SYNC', font=('Arial', 12))
vsync_label.place(x = 212, y=150)

"MAIN LIGHTING"
def updatelight(intensity):
    level = float(intensity)/10
    message = f"6, {level}"
    send_udp_message(message)
lightslider = Scale(root, from_=0, to=80, orient=HORIZONTAL, length= 150, command= updatelight)
lightslider.place(x=190, y=250)
mainlight_label = Label(root, text='Main Lighting', font=('Arial', 12))
mainlight_label.place(x = 210, y=230)

"ENVIROMENT LIGHTING"
def updateenvi(intensity):
    level = float(intensity)/10
    message = f"7, {level}"
    send_udp_message(message)
envislider = Scale(root, from_=0, to=80, orient=HORIZONTAL, length= 150, command= updateenvi)
envislider.place(x=190, y=320)
envilight_label = Label(root, text='Enviroment Lighting', font=('Arial', 12))
envilight_label.place(x = 195, y=300)

"COLOR CORRECTION"
def updatecolor(dump):
    saturation = float(saturslider.get())
    contrast = float(contraslider.get())
    exposition = float(exposlider.get())
    message = f"8, {saturation}, {contrast}, {exposition}"
    send_udp_message(message)
saturslider = Scale(root, from_=-100, to=100, orient=HORIZONTAL, length= 150, command= updatecolor)
saturslider.place(x=350, y=30)
saturlabel = Label(root, text='Saturation', font=('Arial', 12))
saturlabel.place(x = 387, y=10)

contraslider = Scale(root, from_=-100, to=100, orient=HORIZONTAL, length= 150, command= updatecolor)
contraslider.place(x=350, y=100)
contralabel = Label(root, text='Contrast', font=('Arial', 12))
contralabel.place(x = 390, y=80)

exposlider = Scale(root, from_=-100, to=100, orient=HORIZONTAL, length= 150, command= updatecolor)
exposlider.place(x=350, y=170)
expolabel = Label(root, text='Exposure', font=('Arial', 12))
expolabel.place(x = 387, y=150)

"RENDER DISTANCE"
def updatefarclip(dump):
    farclip = (float(farclipslider.get()))
    message = f"9, {farclip}"
    send_udp_message(message)

farclipslider = Scale(root, from_=1, to=1000, orient=HORIZONTAL, length= 150, command= updatefarclip)
farclipslider.place(x=350, y=240)
farcliplabel = Label(root, text='Render Distance', font=('Arial', 12))
farcliplabel.place(x = 367, y=220)

"SHADOW QUALITY"
def updateshadows(dump):
    print("button")
    shadist = float(shadowslider.get())
    shadval = currentshadow.get()
    message = f"10, {shadist}, {shadval} "
    send_udp_message(message)
def redoshadows():
    updateshadows(1)

shadowlist = ["hard", "soft", "off"]
currentshadow = StringVar(root)
currentshadow.set(shadowlist[0])
shadowslider = Scale(root, from_=0, to=500, orient=HORIZONTAL, length= 150, command= updateshadows)
shadowslider.place(x=350, y=300)
shadowlabel = Label(root, text='Shadow Distance', font=('Arial', 12))
shadowlabel.place(x = 367, y=280)
shadowselect = OptionMenu(root, currentshadow, *shadowlist)
shadowselect.place(x = 350, y = 340)
shadowbtn = Button(root, text="Update", command= redoshadows)
shadowbtn.place(x=430, y=342)

"BLOOM"
def updatebloom(dump):
    current_value = float(bloomslider.get())
    bloom_int = 1 if bloom_is_on else 0
    message = f"11, {bloom_int}, {current_value}"
    send_udp_message(message)

def togglebloom():
    global bloom_is_on
    bloom_is_on = not bloom_is_on
    if bloom_is_on:
        bloombtn.config(text='ON', bg='green', fg='white')
    else:
        bloombtn.config(text='OFF', bg='red', fg='white')
    updatebloom(1)

bloomslider = Scale(root, from_=0, to=100, orient=HORIZONTAL, length= 150, command= updatebloom)
bloomslider.place(x=510, y=90)
bloombtn = Button(root, text='ON', bg='green', fg='white', font=('Arial', 16), command=togglebloom, width=10)
bloombtn.place(x=525, y=50)
bloomlabel = Label(root, text='Bloom', font=('Arial', 12) )
bloomlabel.place(x=560, y=20)

"MOTION BLUR"

def updatemotion():
    global motion_on
    motion_on = not motion_on
    if motion_on:
        motion_btn.config(text='ON', bg='green', fg='white')
    else:
        motion_btn.config(text='OFF', bg='red', fg='white')
    motion_int = 1 if motion_on else 0
    message = f"12, {motion_int}"
    send_udp_message(message)

motion_btn = Button(root, text='OFF', bg='red', fg='white', font=('Arial', 16), command=updatemotion, width=10)
motion_btn.place(x=525, y=175)
motion_label = Label(root, text='Motion Blur', font=('Arial', 12))
motion_label.place(x = 545, y=145)

"SSAO"

def updatessao(dump):
    ssao_int = 1 if ssao_is_on else 0
    message = f"13, {ssao_int}"
    send_udp_message(message)

def togglessao():
    global ssao_is_on
    ssao_is_on = not ssao_is_on
    if ssao_is_on:
        ssaobtn.config(text='ON', bg='green', fg='white')
    else:
        ssaobtn.config(text='OFF', bg='red', fg='white')
    updatessao(1)
ssaobtn = Button(root, text='ON', bg='green', fg='white', font=('Arial', 16), command=togglessao, width=10)
ssaobtn.place(x=525, y=255)
ssaolabel = Label(root, text='SSAO', font=('Arial', 12) )
ssaolabel.place(x=560, y=230)

"DEPTH OF FIELD"
def updatedepth(dump):
    depthlevel = float(depthslider.get())
    message = f"14, {depthlevel}"
    send_udp_message(message)
depthslider = Scale(root, from_=1, to=100, orient=HORIZONTAL, length= 150, command= updatedepth)
depthslider.place(x=680, y=30)
depthlabel = Label(root, text='Depth of Field', font=('Arial', 12))
depthlabel.place(x = 705, y=10)

"VIGNETTE"
def updatevign(dump):
    vignlevel = float(vignslider.get())
    message = f"15, {vignlevel}"
    send_udp_message(message)

vignslider = Scale(root, from_=0, to=100, orient=HORIZONTAL, length= 150, command= updatevign)
vignslider.place(x=680, y=90)
vignlabel = Label(root, text='Vignette', font=('Arial', 12))
vignlabel.place(x = 718, y=73)

"FOG"

def updatefog(dump):
    current_value = float(fogslider.get())
    fog_int = 1 if fog_is_on else 0
    message = f"16, {fog_int}, {current_value}"
    send_udp_message(message)

def togglefog():
    global fog_is_on
    fog_is_on = not fog_is_on
    if fog_is_on:
        fogbtn.config(text='ON', bg='green', fg='white')
    else:
        fogbtn.config(text='OFF', bg='red', fg='white')
    updatefog(1)

fogslider = Scale(root, from_=0.00, to=100, orient=HORIZONTAL, length= 150, command= updatefog)
fogslider.place(x=680, y=210)
fogbtn = Button(root, text='OFF', bg='red', fg='white', font=('Arial', 16), command=togglefog, width=10)
fogbtn.place(x=690, y=170)
foglabel = Label(root, text='Fog', font=('Arial', 12) )
foglabel.place(x=730, y=140)

root.mainloop()