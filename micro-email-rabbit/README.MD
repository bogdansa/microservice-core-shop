```
docker run --name micro-email -d -p 4000:4000 workshop/micro-email 4000
```

to Query

```
http POST localhost:4000/email emailAddress=lam.michal.franc@gmail.com message=test -v
http localhost:4000/status/logs
```
