import youtube_dl, sys

if len(sys.argv) == 1:
	print("No value")
	sys.exit()
value = sys.argv[1]
ydl = youtube_dl.YoutubeDL()
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
	output = video['url']
except Exception as err:
	output = err.args[0]
	print("ERROR")
file = open("file.txt", 'w')
file.write(output)
file.close()