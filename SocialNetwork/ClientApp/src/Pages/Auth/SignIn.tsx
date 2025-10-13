import { useState } from "react";
import { NavLink } from "react-router";
import authService from "../../Services/AuthorizeService";
import Button from "@mui/material/Button";
import TextField from "@mui/material/TextField";
import Box from "@mui/material/Box";
import Grid from "@mui/material/Grid";

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
  let data: loginData = {
    UserName: getInputValue("Identifier"),
    Email: getInputValue("Identifier"),
    Password: getInputValue("Password"),
  };
  for (let prop in data) {
    if (prop === "UserName" || prop === "Email") prop = "Identifier";
    if (!getElementById<HTMLInputElement>(prop).reportValidity()) {
      getElementById<HTMLInputElement>(prop).focus();
      return;
    }
  }
  let response = await fetch(`api/Account/SignIn`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
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
      Password: "",
    };
    for (const prop in modelState) {
      errors[prop] = modelState[prop][0];
    }
    setErrors(errors);
  }
};

export default function Login() {
  let errors: loginData | undefined;
  [errors, setErrors] = useState<loginData>();
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
    <Box padding={3}>
      <Grid container spacing={2}>
        <Grid size={12}>
          <TextField
            id="Identifier"
            label="Username or Email"
            helperText={errors ? errors.UserName + errors.Email : ""}
            error={
              errors !== undefined &&
              (errors.UserName !== undefined || errors.Email !== undefined) &&
              (errors.UserName !== "" || errors.Email !== "")
            }
            onChange={changeHandle}
            fullWidth
          />
        </Grid>
        <Grid size={12}>
          <TextField
            id="Password"
            type="Password"
            label="Password"
            required
            helperText={errors?.Password}
            error={errors?.Password !== undefined && errors?.Password !== ""}
            onChange={changeHandle}
            fullWidth
          />
        </Grid>
        <Grid size={12}>
          <Button
            variant="contained"
            color="primary"
            fullWidth
            onClick={async () => await signIn()}
          >
            Login
          </Button>
        </Grid>
        <Grid size={12}>
          <NavLink to="/SignUp" style={{ textDecoration: "none" }}>
            <Button>SignUp</Button>
          </NavLink>
        </Grid>
      </Grid>
    </Box>
  );
}
