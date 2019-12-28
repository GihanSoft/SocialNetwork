import React from "react";
import {
    TextField,
    Button,
    Grid,
    makeStyles,
    Theme,
    createStyles
} from "@material-ui/core";
import Config from "../../Services/Config";
import { NavLink } from "react-router-dom";
import authService from "../../Services/AuthorizeService";

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        root: {
            padding: theme.spacing(3)
        }
    })
);

interface loginData {
    UserName: string;
    Email: string;
    Password: string;
    [key: string]: string;
}

function getElementById<T extends HTMLElement>(id: string): T {
    return document.getElementById(id) as T;
}

function getInputValue(id: string): string {
    return getElementById<HTMLInputElement>(id).value;
}

let setErrors: React.Dispatch<React.SetStateAction<loginData | undefined>>;

const signIn = async () => {
    const config = Config;
    let data: loginData = {
        UserName: getInputValue("Identifier"),
        Email: getInputValue("Identifier"),
        Password: getInputValue("Password")
    };
    for (let prop in data) {
        if (prop === "UserName" || prop === "Email") prop = "Identifier";
        if (!getElementById<HTMLInputElement>(prop).reportValidity()) {
            getElementById<HTMLInputElement>(prop).focus();
            return;
        }
    }
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
        window.location.assign("/");
        return;
    } else {
        let modelState: { [key: string]: string[] } = await response.json();
        let errors: loginData = {
            UserName: "",
            Email: "",
            Password: ""
        };
        for (const prop in modelState) {
            errors[prop] = modelState[prop][0];
        }
        setErrors(errors);
    }
};

export default function Login() {
    const classes = useStyles();
    let errors: loginData | undefined;
    [errors, setErrors] = React.useState<loginData>();
    if (authService.isSigned()) {
        window.location.assign("/");
        return <></>;
    }
    const changeHandle = (event: React.ChangeEvent<HTMLInputElement>) => {
        let id = event.target.id;
        let nErr = { ...errors } as loginData | undefined;
        if (nErr) {
            if (id === "Identifier") {
                nErr.UserName = "";
                nErr.Email = "";
            } else nErr[id] = "";
            setErrors(nErr);
        }
    };
    return (
        <div className={classes.root}>
            <Grid container spacing={2}>
                <Grid item xs={1} sm={2} md={4}></Grid>
                <Grid item xs={12} sm={8} md={4}>
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <TextField
                                id="Identifier"
                                label="Username or Email"
                                helperText={
                                    errors?.UserName || "" + errors?.Email || ""
                                }
                                error={
                                    errors?.UserName !== undefined &&
                                    errors?.Email !== undefined &&
                                    errors?.UserName !== "" &&
                                    errors?.Email !== ""
                                }
                                onChange={changeHandle}
                                fullWidth
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                id="Password"
                                type="Password"
                                label="Password"
                                required
                                helperText={errors?.Password}
                                error={
                                    errors?.Password !== undefined &&
                                    errors?.Password !== ""
                                }
                                onChange={changeHandle}
                                fullWidth
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <Button
                                variant="contained"
                                color="primary"
                                fullWidth
                                onClick={async () => await signIn()}
                            >
                                Login
                            </Button>
                        </Grid>
                        <Grid item xs={12}>
                            <NavLink
                                to="/SignUp"
                                style={{ textDecoration: "none" }}
                            >
                                <Button>SignUp</Button>
                            </NavLink>
                        </Grid>
                    </Grid>
                </Grid>
                <Grid item xs={1} sm={2} md={4}></Grid>
            </Grid>
        </div>
    );
}
