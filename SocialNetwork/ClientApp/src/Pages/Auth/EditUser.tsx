import React from "react";
import {
    TextField,
    Button,
    Grid,
    makeStyles,
    Theme,
    createStyles,
    FormControlLabel,
    Checkbox
} from "@material-ui/core";
import Config from "../../Services/Config";
import config from "../../Services/Config";
import authService from "../../Services/AuthorizeService";

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        root: {
            padding: theme.spacing(3)
        }
    })
);

interface loginData {
    userName: string;
    email: string;
    mobile: string;
    oldPassword?: string;
    password?: string;
    passwordConf?: string;
    isPrivate: boolean | string;
    [key: string]: boolean | string | undefined;
}

function getElementById<T extends HTMLElement>(id: string): T {
    return document.getElementById(id) as T;
}

function getInputValue(id: string): string {
    return getElementById<HTMLInputElement>(id).value;
}

let setErrors: React.Dispatch<React.SetStateAction<loginData | undefined>>;

const editUser = async () => {
    const config = Config;
    let data: loginData = {
        userName: getInputValue("userName"),
        email: getInputValue("email"),
        mobile: getInputValue("mobile"),
        oldPassword: getInputValue("oldPassword"),
        password: getInputValue("password"),
        passwordConf: getInputValue("passwordConf"),
        isPrivate: getElementById<HTMLInputElement>("isPrivate").checked
    };
    for (const prop in data) {
        if (!getElementById<HTMLInputElement>(prop).reportValidity()) {
            getElementById<HTMLInputElement>(prop).focus();
            return;
        }
    }
    let response = await fetch(`${config.apiBase}/Account/Edit`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("AuthJWT")}`
        },
        body: JSON.stringify(data)
    });
    if (response.ok) {
        window.location.assign("/SignIn");
        return;
    } else {
        if (response.status == 400) {
            let modelState: { [key: string]: string[] } = await response.json();
            let errors: loginData = {
                userName: "",
                email: "",
                mobile: "",
                oldPassword: "",
                password: "",
                passwordConf: "",
                isPrivate: ""
            };
            for (const prop in modelState) {
                let ccProp = prop[0].toLowerCase() + prop.slice(1, prop.length);    
                errors[ccProp] = modelState[prop][0];
            }
            setErrors(errors);
        }
    }
};

export default function EditUser() {
    //todo get default values

    const classes = useStyles();
    let errors: loginData | undefined;
    [errors, setErrors] = React.useState<loginData>();
    let defVal: loginData | undefined;
    if (authService.isSigned()) {
        let xhr = new XMLHttpRequest();
        xhr.open(
            "POST",
            `${config.apiBase}/User/${authService.isSigned()}`,
            false
        );
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.setRequestHeader(
            "Authorization",
            `Bearer ${localStorage.getItem("AuthJWT")}`
        );
        xhr.send();
        if (xhr.status === 200) {
            defVal = JSON.parse(xhr.responseText) as loginData;
        }
    }

    const changeHandle = (event: React.ChangeEvent<HTMLInputElement>) => {
        let id = event.target.id;
        let nErr = { ...errors } as loginData | undefined;
        if (nErr) {
            nErr[id] = "";
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
                                id="userName"
                                label="UserName"
                                defaultValue={defVal?.userName}
                                disabled={true}
                                required
                                helperText={errors?.userName}
                                error={
                                    errors?.userName !== undefined &&
                                    errors?.userName !== ""
                                }
                                onChange={changeHandle}
                                fullWidth
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                id="email"
                                type="email"
                                label="Email"
                                defaultValue={defVal?.email}
                                required
                                helperText={errors?.email}
                                error={
                                    errors?.email !== undefined &&
                                    errors?.email !== ""
                                }
                                onChange={changeHandle}
                                fullWidth
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                id="mobile"
                                label="Mobile Number"
                                defaultValue={defVal?.mobile}
                                required
                                helperText={errors?.mobile}
                                error={
                                    errors?.mobile !== undefined &&
                                    errors?.mobile !== ""
                                }
                                onChange={changeHandle}
                                fullWidth
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                id="oldPassword"
                                type="Password"
                                label="Old Password"
                                helperText={errors?.password}
                                error={
                                    errors?.password !== undefined &&
                                    errors?.password !== ""
                                }
                                onChange={changeHandle}
                                fullWidth
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                id="password"
                                type="Password"
                                label="Password"
                                helperText={errors?.password}
                                error={
                                    errors?.password !== undefined &&
                                    errors?.password !== ""
                                }
                                onChange={changeHandle}
                                fullWidth
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                id="passwordConf"
                                type="Password"
                                label="Password Confirm"
                                helperText={errors?.passwordConf}
                                error={
                                    errors?.passwordConf !== undefined &&
                                    errors?.passwordConf !== ""
                                }
                                onChange={changeHandle}
                                fullWidth
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <FormControlLabel
                                label="Is Private User"
                                control={
                                    <Checkbox
                                        id="isPrivate"
                                        defaultChecked={
                                            typeof defVal?.isPrivate ===
                                            "boolean"
                                                ? (defVal?.isPrivate as boolean)
                                                : undefined
                                        }
                                    />
                                }
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <Button
                                variant="contained"
                                color="primary"
                                fullWidth
                                onClick={() =>
                                    editUser().then(() =>
                                        console.log("fullfil")
                                    )
                                }
                            >
                                Apply Edit
                            </Button>
                        </Grid>
                    </Grid>
                </Grid>
                <Grid item xs={1} sm={2} md={4}></Grid>
            </Grid>
        </div>
    );
}
