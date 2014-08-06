# Feeds Notifier

## Troubleshooting

### Problem:

How to install Windows SMTP Server?

### Solution:

    powershell -command "Set-ExecutionPolicy Unrestricted"
    Import-Module Servermanager
    Add-WindowsFeature SMTP-Server

Source: http://richardprodger.wordpress.com/2011/07/18/using-the-windows-smtp-server-in-azure/

### Problem:

The server response was: 5.7.1 Unable to relay for <outgoing address>

### Solution:

1. Go to Administrative Tools -> IIS 6.0 Manager
2. Right click "SMTP Virtual Server" and "properties"
3. Select Access tab
4. Click "Relay..." in Relay Restrictions
5. Select "Only the list below"
6. Add 127.0.0.1 and my server IP to the list
7. Check Allow all computers which successfully authenticate to relay, regardless of the list above
8. Click "OK"

### Gmail Is Blocking Emails

Gmail blocks when sending email and it recommends to follow the following rules: https://support.google.com/mail/answer/81126?hl=en

### Solution:

Apparently using a from address like blah@kiewic.com is more trustworthy then blah@http2.cludapp.net


