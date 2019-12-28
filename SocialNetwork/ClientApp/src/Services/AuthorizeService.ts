import Config from "./Config";

export class AuthorizeService {
    public isSigned(): string | undefined {
        let token = localStorage.getItem("AuthJWT");
        if (token === null) return undefined;
        try {
            let base64Url = token.split(".")[1];
            let base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
            let jsonPayload = decodeURIComponent(
                atob(base64)
                    .split("")
                    .map(function(c) {
                        return (
                            "%" +
                            ("00" + c.charCodeAt(0).toString(16)).slice(-2)
                        );
                    })
                    .join("")
            );
            return JSON.parse(jsonPayload).unique_name;
        } catch {
            localStorage.removeItem("AuthJWT");
            return undefined;
        }
    }

    public async signIn(
        identifier: string,
        password: string,
        remember: boolean
    ): Promise<boolean> {
        const config = Config;
        let data = {
            Username: identifier,
            Email: identifier,
            Password: password,
            RememberMe: remember
        };
        let response = await fetch(`${config.apiBase}/Account/SignIn`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data)
        });
        if (response.ok) {
            let token = await response.text();
            localStorage.setItem("AuthJWT", token);
            return true;
        }
        return false;
    }
}

const authService = new AuthorizeService();

export default authService;
