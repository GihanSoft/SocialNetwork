import Icon from "@mdi/react";
import { mdiMagnify } from "@mdi/js";
import PostList from "../Components/PostList";
import { NavLink } from "react-router";
import { useState } from "react";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import InputAdornment from "@mui/material/InputAdornment";
import { useTheme } from "@mui/material/styles";

const getUsers = async (
  s: string
): Promise<{ username: string; link: string }[]> => {
  let response = await fetch(`api/User/Search?s=${s}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${localStorage.getItem("AuthJWT")}`,
    },
  });
  let users: { userName: string }[] = await response.json();
  return users.map((v) => {
    return { username: v.userName, link: `/User/${v.userName}` };
  });
};

export default function Search() {
  let [query, setQuery] = useState<string>();
  let [users, SetUserSearch] = useState<{ username: string; link: string }[]>();
  let intervalHandle: NodeJS.Timeout | undefined;
  const theme = useTheme();

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
            let s = (document.getElementById("search") as HTMLInputElement)
              .value;
            setQuery(s);
            getUsers(s).then((u) => {
              SetUserSearch(u);
            });
            if (intervalHandle) {
              clearInterval(intervalHandle);
            }
          }, 1000 * 0.5);
        }}
        style={{ margin: 10, width: "calc(100% - 20px)" }}
        slotProps={{
          input: {
            startAdornment: (
              <InputAdornment position="start">
                <Icon
                  path={mdiMagnify}
                  size={1.25}
                  style={{
                    fill: theme.palette.primary.main,
                    color: theme.palette.primary.main,
                  }}
                />
              </InputAdornment>
            ),
          },
        }}
      />
      {users
        ? users.map((v) => {
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
      {query ? <PostList url={`/API/Post/Search?s=${query}`} /> : ""}
    </>
  );
}
