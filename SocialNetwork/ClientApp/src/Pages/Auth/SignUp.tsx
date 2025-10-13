import { useState } from "react";

import Grid from "@mui/material/Grid";
import Box from "@mui/material/Box";
import TextField from "@mui/material/TextField";
import FormControlLabel from "@mui/material/FormControlLabel";
import Checkbox from "@mui/material/Checkbox";
import Button from "@mui/material/Button";

interface loginData {
  UserName: string;
  Email: string;
  Mobile: string;
  Password: string;
  PasswordConf: string;
  IsPrivate: boolean | string;
  [key: string]: boolean | string;
}

function getElementById<T extends HTMLElement>(id: string): T {
  return document.getElementById(id) as T;
}

function getInputValue(id: string): string {
  return getElementById<HTMLInputElement>(id).value;
}

let setErrors: React.Dispatch<React.SetStateAction<loginData | undefined>>;

const signUp = async () => {
  let data: loginData = {
    UserName: getInputValue("UserName"),
    Email: getInputValue("Email"),
    Mobile: getInputValue("Mobile"),
    Password: getInputValue("Password"),
    PasswordConf: getInputValue("PasswordConf"),
    IsPrivate: getElementById<HTMLInputElement>("IsPrivate").checked,
  };
  for (const prop in data) {
    if (!getElementById<HTMLInputElement>(prop).reportValidity()) {
      getElementById<HTMLInputElement>(prop).focus();
      return;
    }
  }
  let response = await fetch(`api/Account/SignUp`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
  if (response.ok) {
    window.location.assign("/SignIn");
    return;
  } else {
    let modelState: { [key: string]: string[] } = await response.json();
    let errors: loginData = {
      UserName: "",
      Email: "",
      Mobile: "",
      Password: "",
      PasswordConf: "",
      IsPrivate: "",
    };
    for (const prop in modelState) {
      errors[prop] = modelState[prop][0];
    }
    setErrors(errors);
  }
};

export default function SignUp() {
  let errors: loginData | undefined;
  [errors, setErrors] = useState<loginData>();
  const changeHandle = (event: React.ChangeEvent<HTMLInputElement>) => {
    let id = event.target.id;
    let nErr = { ...errors } as loginData | undefined;
    if (nErr) {
      nErr[id] = "";
      setErrors(nErr);
    }
  };
  return (
    <Box padding={3}>
      <Grid container spacing={2}>
        <Grid size={{ xs: 1, sm: 2, md: 2 }}></Grid>
        <Grid size={{ xs: 12, sm: 8, md: 8 }}>
          <Grid container spacing={2}>
            <Grid size={12}>
              <TextField
                id="UserName"
                label="UserName"
                required
                helperText={errors?.UserName}
                error={
                  errors?.UserName !== undefined && errors?.UserName !== ""
                }
                onChange={changeHandle}
                fullWidth
              />
            </Grid>
            <Grid size={12}>
              <TextField
                id="Email"
                type="Email"
                label="Email"
                required
                helperText={errors?.Email}
                error={errors?.Email !== undefined && errors?.Email !== ""}
                onChange={changeHandle}
                fullWidth
              />
            </Grid>
            <Grid size={12}>
              <TextField
                id="Mobile"
                label="Mobile Number"
                required
                helperText={errors?.Mobile}
                error={errors?.Mobile !== undefined && errors?.Mobile !== ""}
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
                error={
                  errors?.Password !== undefined && errors?.Password !== ""
                }
                onChange={changeHandle}
                fullWidth
              />
            </Grid>
            <Grid size={12}>
              <TextField
                id="PasswordConf"
                type="Password"
                label="PasswordConf"
                required
                helperText={errors?.PasswordConf}
                error={
                  errors?.PasswordConf !== undefined &&
                  errors?.PasswordConf !== ""
                }
                onChange={changeHandle}
                fullWidth
              />
            </Grid>
            <Grid size={12}>
              <FormControlLabel
                label="Is Private User"
                control={<Checkbox id="IsPrivate" />}
              />
            </Grid>
            <Grid size={12}>
              <Button
                variant="contained"
                color="primary"
                fullWidth
                onClick={() => signUp().then(() => console.log("fullfil"))}
              >
                Register
              </Button>
            </Grid>
          </Grid>
        </Grid>
        <Grid size={{ xs: 1, sm: 2, md: 2 }}></Grid>
      </Grid>
    </Box>
  );
}
