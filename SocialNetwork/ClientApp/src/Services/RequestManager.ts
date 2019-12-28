
export class RequestManager {
    baseUrl?: string;
    defaultHeaders?: { [id: string]: string };

    constructor(baseUrl?: string, defaultHeaders?: { [id: string]: string }) {
        if (baseUrl)
            while (baseUrl.endsWith("/"))
                baseUrl = baseUrl.substr(0, baseUrl.length - 1);
        this.baseUrl = baseUrl;
        this.defaultHeaders = defaultHeaders;
    }

    public async Send(
        url: string,
        method: string,
        params?: object | any,
        headers?: { [id: string]: string },
        callback?: (xhr: XMLHttpRequest) => void
    ) {
        let xhr = new XMLHttpRequest();
        while (url.startsWith("/")) url = url.substr(1);
        url = this.baseUrl + "/" + url;
        if (params && method === "GET") {
            var Url = new URL(url);
            let parameters = params as { [id: string]: any };
            for (const key in parameters) {
                Url.searchParams.append(key, parameters[key]);
            }
            url = Url.href;
        }
        xhr.open(method, url, true);
        xhr.onloadend = e => {
            if (callback) callback(xhr);
        };
        if (this.defaultHeaders)
            for (const key in this.defaultHeaders) {
                xhr.setRequestHeader(key, this.defaultHeaders[key]);
            }
        // console.log(authService.isAuthenticated());
        // console.log(authService._user || "");

        // let token = await authService.getAccessToken();
        let token = localStorage.getItem('AuthJWT');
        if (token) xhr.setRequestHeader("Authorization", `Bearer ${token}`);
        if (headers) {
            for (const key in headers) {
                xhr.setRequestHeader(key, headers[key]);
            }
        }

        let body =
            method === "POST" && typeof params === "object"
                ? JSON.stringify(params)
                : params;
        if (method === "POST")
            xhr.setRequestHeader("Content-Type", "application/json");
        xhr.send(body);
    }
    public Get(
        url: string,
        callback?: (xhr: XMLHttpRequest) => void,
        params?: { [id: string]: string },
        headers?: { [id: string]: string }
    ) {
        this.Send(url, "GET", params, headers, callback);
    }
    public Post(
        url: string,
        callback?: (xhr: XMLHttpRequest) => void,
        params?: object | any,
        headers?: { [id: string]: string }
    ) {
        this.Send(url, "POST", params, headers, callback);
    }
}
