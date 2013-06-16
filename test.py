'''
Created on 2013-5-4

@author: kingo
'''
import sys, time, os, re, urllib, json
from http import cookiejar
from urllib import request, parse

params = {"source":"radio",} #name:value
params['alias']=input('请输入邮箱：')
params['form_password']=input('请输入密码：')

listfile = open('list.txt','w',encoding='utf-8')

cookie = cookiejar.CookieJar()
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cookie))

Mainurl = 'http://douban.fm/mine'
suburl = 'http://douban.fm/common_login?redir=/mine'
req = urllib.request.urlopen(suburl)
html = req.read().decode('utf8')

#get captcha_id
captchaurl = 'http://douban.fm/j/new_captcha'
data = urllib.parse.urlencode({'ck':'null'}).encode('unicode_escape')
response=opener.open(captchaurl, data)
params['captcha_id']=response.read().decode('utf8').split('\"')[1]
                                                          
#get captcha_solution
requrl = 'http://www.douban.com/misc/captcha?size=m&id=%s' % params['captcha_id']
#方法一：
'''
response=opener.open(requrl)
imgdata = response.read()
imgfile = open('v.jpg', "wb")
imgfile.write(imgdata)
imgfile.close()
'''
#方法二：
urllib.request.urlretrieve(requrl,'v.jpg')
vcode=input('请输入图片上的验证码：')
params['captcha_solution']=vcode

#login
loginurl= 'http://douban.fm/j/login'
data = urllib.parse.urlencode(params).encode('unicode_escape')
response = opener.open(loginurl,data)
ans = json.loads(response.read().decode('utf8'))
if 'err_no' in ans:
        print('登录错误：%s' % ans['err_msg'])
        exit(0)
else:
        print ('登陆成功')
        print('累计收听：%s 首' % ans['user_info']['play_record']['played'])
        print('加红心：%s 首' % ans['user_info']['play_record']['liked'])
        print('不再收听：%s 首' % ans['user_info']['play_record']['banned'])
        
#抓取红心歌曲表单
url = Mainurl #第一页歌曲列表url
while(True):
        html = opener.open(url).read().decode('utf8')
        songlist = re.findall(
                '''  <div class="props">
                        <p class="song_title">(.+?)</p>
                        <p class="performer">(.+?)</p>
                        <p class="source">
                            <a
                            href="(.+?)" target="_blank"
                            >(.+?)</a>
                        </p>
                    </div>''',html)
        for song in songlist:
                imgname = song[0]+'-'+song[1]+'-'+song[3]
                imgname=imgname.replace('/','&')
                print(imgname)
                listfile.write(imgname+'\n') 
                ##################################################
                ###############     批量下载封面     ##################
                ##################################################         
                imghtml = urllib.request.urlopen(song[2]).read().decode('utf8')
                imgurl = re.search('''<a class="nbg" href="(.+?)"
           title="点击看大图">''',imghtml).group(1)  
                urllib.request.urlretrieve(imgurl,imgname+'.jpg')
                ##################################################
                
        ans=re.findall('''<span class="next">
            <link rel="next" href="(.+?)"/>
            <a href=".*?" >后页&gt;</a>
        </span>''',html)
        #如果是最后一页
        if len(ans) == 0:
                print ('over')
                listfile.close()
                break
        #如果还有下一页
        else:
                url = 'http://douban.fm/' + ans[0]
    
