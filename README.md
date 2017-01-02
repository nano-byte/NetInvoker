NetInvoker
==========

Provides an HTTP interface for invoking commands in a remote Windows session.

Configuration
-------------
The configuration is stored in [`app.config` / `NetInvoker.exe.config`](https://msdn.microsoft.com/en-us/library/8eyb2ct1.aspx).

Default values:
```
Port: 8888
Username: netinvoker
Password: netinvoker
```

You may need to execute the following commands once as Administrator before using NetInvoker:
```
netsh http add urlacl http://+:8888/ user=%COMPUTERNAME%\%USERNAME%
netsh advfirewall firewall add rule name="NetInvoker" protocol=TCP dir=in localport=8888 action=allow
```

Usage
-----
Navigate to `http://remotehost:8888/`. Use the web form or add the bookmarklet link to your bookmarks.

REST Interface:
```
GET /?fileName=notepad.exe&arguments=myfile.txt

POST /
fileName=notepad.exe&arguments=myfile.txt
```
The `fileName` is mandatory, the `arguments` parameter is optional.
