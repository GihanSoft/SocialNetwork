import React from "react";
import {
    TextField,
    InputAdornment,
    makeStyles,
    Theme,
    createStyles,
    Button
} from "@material-ui/core";
import Icon from "@mdi/react";
import { mdiMagnify } from "@mdi/js";
import PostList from "../Components/PostList";
import config from "../Services/Config";
import { NavLink } from "react-router-dom";

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        icon: {
            color: theme.palette.common.black,
            fill: theme.palette.common.black
        }
    })
);

const getUsers = async (
    s: string
): Promise<{ username: string; link: string }[]> => {
    let response = await fetch(`${config.apiBase}/User/Search?s=${s}`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("AuthJWT")}`
        }
    });
    let users: { userName: string }[] = await response.json();
    return users.map(v => {
        return { username: v.userName, link: `/User/${v.userName}` };
    });
};
export default function Search() {
    let [s, setS] = React.useState<string>();
    let [users, SetUserSearch] = React.useState<
        { username: string; link: string }[]
    >();
    let intervalHandle: NodeJS.Timeout | undefined;
    const classes = useStyles();
    return (
        <>
            <TextField
                id="search"
                type="search"
                fullWidth
                onKeyUp={() => {
                    if (intervalHandle) {
                        clearInterval(intervalHandle);
                    }
                    intervalHandle = setInterval(() => {
                        let s = (document.getElementById(
                            "search"
                        ) as HTMLInputElement).value;
                        setS(s);
                        getUsers(s).then(u => {
                            SetUserSearch(u);
                        });
                        if (intervalHandle) {
                            clearInterval(intervalHandle);
                        }
                    }, 1000 * 0.5);
                }}
                style={{ margin: 10, width: "calc(100% - 20px)" }}
                InputProps={{
                    startAdornment: (
                        <InputAdornment position="start">
                            <Icon
                                path={mdiMagnify}
                                size={1.25}
                                className={classes.icon}
                            />
                        </InputAdornment>
                    )
                }}
            />
            {users
                ? users.map(v => {
                      return (
                          <>
                              <NavLink to={v.link}>
                                  <Button fullWidth style={{ padding: 5 }}>
                                      {v.username}
                                  </Button>
                              </NavLink>
                              <br />
                          </>
                      );
                  })
                : ""}
            {s ? <PostList url={`/API/Post/Search?s=${s}`} /> : ""}
        </>
    );
}
