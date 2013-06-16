# -*- coding: gb2312 -*-
import urllib.request
def getContent(url): 
  f=urllib.request.urlopen(url)
  page=f.read()
  page=page.decode('gbk','ignore');
  return page

def getUrl(page):
  start_link=page.find('<a href=')
  start_quote=page.find('"',start_link)
  end_quote=page.find('"',start_quote+1)
  url=page[start_quote+1:end_quote]
  page=page[end_quote:]
  return url

def storefile(url,page):
  f=open('url','w')
  f.write(page)
  f.close()

def fetchwidth(url,width,deepth):
  i=0
  page=getContent(url)
  while(i<width):
    start_link=page.find('<a href=')
    start_quote=page.find('"',start_link)
    end_quote=page.find('"',start_quote+1)
    url=page[start_quote+1:end_quote]
    s=getContent(url)
    fetchdepth(url,deepth)
    storefile(url,s)
    page=page[end_quote:]
    i=i+1 

def fetchdepth(url,deepth):
  i=0
  page=getContent(url)
  while(i<deepth):
    start_link=page.find('<a href=')
    start_quote=page.find('"',start_link)
    end_quote=page.find('"',start_quote+1)
    url=page[start_quote+1:end_quote]
    page=getContent(url)
    storefile(url,page)
    i=i+1 
    
print ("todos")
fetchwidth('http://www.baidu.com',2,2)