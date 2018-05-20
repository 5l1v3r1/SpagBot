import io, sys, socket, time, youtube_dl, json, subprocess

def GetYoutubeLink(value):
    ydl = youtube_dl.YoutubeDL({
    	"format": 'best'  # choice of quality
	})
    try:
        with ydl:
            result = ydl.extract_info(
                value,
                    download=False
                )
        if 'entries' in result:
            video = result['entries'][0]
        else:
            video = result
            if "m3u8" in video['url']:
                VideoType = "livestream"
            else:
                VideoType = "video"
        output = { "url" : video['url'], "title" : video['title'], "type" : VideoType }
    except Exception as err:
        output = { "errorset" : err.args[0] }
        print(err.args[0])
    return json.dumps(output)

def RestartSpagbot():
    subprocess.call("~/run.sh", shell=True)
    sys.exit() #kill process so .NET can spawn another one


TCP_IP = '127.0.0.1'
TCP_PORT = 1212
BUFFER_SIZE = 2000

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind((TCP_IP, TCP_PORT))
s.listen(1)

while 1:
    conn, addr = s.accept()
    print ('Connection Engaged', addr)
    data = conn.recv(BUFFER_SIZE)
    if not data: break
    datadecoded = data.decode("utf-8")
    if datadecoded == 'restartme':
        RestartSpagbot()
    else:
        url = GetYoutubeLink(datadecoded)
        conn.send(url.encode())
        conn.close()
