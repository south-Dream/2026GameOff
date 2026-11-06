from pathlib import Path
import numpy as np
from scipy import signal
from scipy.io import wavfile

rate=44100
rng=np.random.default_rng(20261106)
root=Path('Assets/Resources')

def save(path,a,peak=.75):
    path=Path(path);path.parent.mkdir(parents=True,exist_ok=True)
    a=np.asarray(a,dtype=np.float64)
    a-=a.mean(axis=0)
    maximum=np.max(np.abs(a))
    a*=peak/max(maximum,1e-9)
    wavfile.write(path,rate,np.round(a*32767).astype(np.int16))
    return {'file':str(path),'seconds':round(len(a)/rate,3),'peak':round(float(np.max(np.abs(a))),3),'jump':round(float(np.max(np.abs(a[-1]-a[0]))),7)}

def tone(midi,seconds,kind='key'):
    t=np.arange(int(seconds*rate))/rate
    f=440*2**((midi-69)/12)
    if kind=='pad':
        a=(np.sin(2*np.pi*f*t)+.35*np.sin(2*np.pi*f*1.0012*t)+.12*np.sin(2*np.pi*2*f*t))/1.47
        env=np.minimum(t/.8,1)*np.minimum((seconds-t)/1.1,1)
        return a*np.maximum(env,0)*.085
    if kind=='bell':
        a=np.sin(2*np.pi*f*t+.55*np.sin(2*np.pi*f*2.003*t)*np.exp(-t*1.3))
        env=(1-np.exp(-t*90))*np.exp(-t*1.4)*np.minimum((seconds-t)/.12,1)
        return a*env*.09
    a=np.sin(2*np.pi*f*t)+.22*np.sin(2*np.pi*f*2.001*t)+.08*np.sin(2*np.pi*f*3.002*t)
    env=(1-np.exp(-t*100))*np.exp(-t*1.8)*np.minimum((seconds-t)/.1,1)
    return a*env*.13

seconds=80;length=rate*seconds;music=np.zeros((length,2))
def put(a,start,pan):
    index=int(start*rate)%length
    a=np.stack([a*np.sqrt((1-pan)/2),a*np.sqrt((1+pan)/2)],axis=1)
    end=index+len(a)
    music[index:min(end,length)]+=a[:min(len(a),length-index)]
    if end>length:music[:end-length]+=a[length-index:]

beat=60/72;bar=4*beat
chords=[[50,57,62,65],[46,53,58,62],[48,55,60,64],[45,52,57,60],[43,50,55,58],[46,53,58,65],[48,55,60,67],[45,52,57,64]]
melody=[74,77,76,72,74,70,72,69,74,77,79,76,74,72,69,72,77,74,76,72,70,74,72,69]
for b in range(24):
    notes=chords[b%8]
    for j,n in enumerate(notes):put(tone(n,bar+1.1,'pad'),b*bar,(j-1.5)*.28)
    put(tone(notes[0]+12,3.1),b*bar+.1,-.25)
    put(tone(melody[b],3.4),b*bar+beat*(1.5 if b%3 else 1),.28)
    if b%2==0:put(tone(notes[2]+12,2.8),b*bar+beat*3,-.4)
    if b%4==3:put(tone(melody[b]+12,4.3,'bell'),b*bar+beat*2,.55)
# Circular room tails maintain the loop rather than fading the whole piece
dry=music.copy()
for delay,gain in [(0.071,.12),(.127,.10),(.223,.08),(.431,.075),(.733,.055),(1.13,.04),(1.71,.028),(2.31,.018)]:
    music+=np.roll(dry,int(delay*rate),axis=0)[:,::-1]*gain
reports=[save(root/'Audio/Bgm/FoldedSky.wav',music,.48)]

folder=root/'Prefab/Map/Common/Mechanism/Audio'
def noise(n,cutoff):
    return signal.sosfilt(signal.butter(2,cutoff,fs=rate,output='sos'),rng.normal(size=n))

t=np.arange(int(.24*rate))/rate
a=np.sin(2*np.pi*(420*t+65*t*t))*np.exp(-t*32)+.3*noise(len(t),2200)*np.exp(-t*70)
a*=np.minimum(t/.003,1)*np.minimum((.24-t)/.025,1)
reports.append(save(folder/'Activate.wav',a,.42))
t=np.arange(int(.5*rate))/rate
a=(np.sin(2*np.pi*310*t)+.45*np.sin(2*np.pi*622*t)+.15*np.sin(2*np.pi*1249*t))*np.exp(-t*12)
a+=noise(len(t),1600)*np.exp(-t*65)*.2
a*=np.minimum(t/.003,1)*np.minimum((.5-t)/.04,1)
reports.append(save(folder/'Arrive.wav',a,.4))

for kind in ['Slide','Rotate','Lift']:
    t=np.arange(2*rate)/rate
    base=noise(len(t),{'Slide':1800,'Rotate':1200,'Lift':900}[kind])
    # Integer cycles and circular crossfade make a quiet stable movement loop
    hum=np.sin(2*np.pi*{'Slide':110,'Rotate':82,'Lift':66}[kind]*t)
    pulse=(.68+.22*np.sin(2*np.pi*{'Slide':4,'Rotate':7,'Lift':2}[kind]*t))
    a=(base*.28+hum*.08)*pulse
    fade=int(.12*rate)
    blend=np.linspace(0,1,fade)
    a[:fade]=a[:fade]*blend+a[-fade:]*(1-blend)
    a[-fade:]=a[:fade]
    a=np.concatenate([a[fade:],a[:fade]])
    a[-1]=a[0]
    reports.append(save(folder/(kind+'Loop.wav'),a,.2))

clear=np.zeros((int(3.4*rate),2))
for i,n in enumerate([74,77,81,86]):
    a=tone(n,2.4,'bell')*.8
    start=int(i*.22*rate);end=min(start+len(a),len(clear))
    pan=(-.2 if i%2==0 else .2)
    clear[start:end,0]+=a[:end-start]*np.sqrt((1-pan)/2)
    clear[start:end,1]+=a[:end-start]*np.sqrt((1+pan)/2)
dry=clear.copy()
for d,g in [(.12,.22),(.28,.14),(.5,.09)]:
    shift=int(d*rate);clear[shift:]+=dry[:-shift,::-1]*g
clear[-int(.25*rate):]*=np.linspace(1,0,int(.25*rate))[:,None]
reports.append(save(root/'Audio/Sfx/LevelPassed.wav',clear,.52))
t=np.arange(int(.09*rate))/rate
a=np.sin(2*np.pi*(920*t-1600*t*t))*np.exp(-t*65)*np.minimum(t/.002,1)*np.minimum((.09-t)/.01,1)
reports.append(save(root/'Prefab/Ui/UIRoot/Audio/Click.wav',a,.3))
import json
Path('Library/AudioBuild.json').write_text(json.dumps(reports,indent=2),encoding='utf-8')
print(json.dumps(reports,indent=2))
