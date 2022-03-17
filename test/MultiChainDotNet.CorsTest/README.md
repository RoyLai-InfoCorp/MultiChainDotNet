# Test CORS

## setup live server at port 12345

-   Open VS code settings CTRL + ,
-   Find Live Server Config
-   Click edit in settings.json
-   "liveServer.settings.port": 12345

## Start live server

-   right click index.html in content panel
-   click Open with live server

## CORS error

-   Open chrome debug console and the error should show:

```
Access to fetch at 'http://localhost:12018/' from origin 'http://127.0.0.1:3000' has been blocked by CORS policy: Response to preflight request doesn't pass access control check: No 'Access-Control-Allow-Origin' header is present on the requested resource.
```

## Change live server to port 3000

Check MC_API_CORS in .env file. It should include the endpoint `http://127.0.0.1:3000`.
Run live server again but at port 3000.
