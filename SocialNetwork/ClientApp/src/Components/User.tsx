import { NavLink } from "react-router";
import { RequestManager } from "../Services/RequestManager";
import { useState } from "react";
import Button from "@mui/material/Button";
import Grid from "@mui/material/Grid";
import ButtonBase from "@mui/material/ButtonBase";
import Avatar from "@mui/material/Avatar";
import Typography from "@mui/material/Typography";

export interface UserProps {
  userName: string;
  isPrivate: boolean;
  isFollowed: boolean;
  isFollowAccepted: boolean;
  isFollowRequested: boolean;
}

export default function User(props: UserProps) {
  let [isFollowed, setIsFollowed] = useState<boolean>(props.isFollowed);
  let [isFollowRequested, setIsFollowRequested] = useState<boolean>(
    props.isFollowRequested
  );
  const toggleFollow = (username: string) => {
    let reqMng = new RequestManager();
    let action = isFollowed ? "/UnFollow/" : "/Follow/";
    reqMng.Post("/User" + action + username, (xhr) => {
      if (xhr.status === 200) {
        setIsFollowed(!isFollowed);
      }
    });
  };

  return (
    <Grid
      container
      sx={(theme) => ({
        width: "calc(100% - 10px)",
        margin: 5,
        [theme.breakpoints.up("sm")]: {
          width: 500,
          margin: "auto",
          marginTop: 5,
          marginBottom: 5,
        },
      })}
      spacing={0}
    >
      <Grid size={props.isFollowRequested ? 6 : 9}>
        <NavLink
          to={`/user/${props.userName}`}
          style={{ textDecoration: "none", color: "black" }}
        >
          <ButtonBase
            sx={{
              width: "100%",
              justifyContent: "unset",
            }}
          >
            <Avatar
              src={`${config.apiBase}/user/avatar/${props.userName}`}
              alt={props.userName[0].toUpperCase()}
            />
            <Typography sx={{ fontSize: "16pt", paddingLeft: 25 }}>
              {props.userName}
            </Typography>
          </ButtonBase>
        </NavLink>
      </Grid>
      {isFollowRequested ? (
        <Grid size={3} sx={{ width: "100%", padding: 1.75 }}>
          <Button
            fullWidth
            sx={{
              color: "green",
              border: "1px solid rgba(0, 128, 0, 0.5)",
              "&:hover": {
                color: "forestgreen",
                border: "1px solid rgba(34, 139, 34, 0.5)",
                backgroundColor: "rgba(0, 128, 0, 0.08)",
              },
            }}
            variant="outlined"
            onClick={() => {
              let reqMng = new RequestManager(config.apiBase);
              reqMng.Post("/User/AcceptFollow/" + props.userName, (xhr) => {
                if (xhr.status === 200) {
                  setIsFollowRequested(false);
                }
              });
            }}
          >
            Accept
          </Button>
        </Grid>
      ) : (
        ""
      )}
      <Grid size={3} sx={{ width: "100%", padding: 1.75 }}>
        <Button
          fullWidth
          color="primary"
          onClick={() => {
            toggleFollow(props.userName);
          }}
          variant={isFollowed ? "contained" : "outlined"}
        >
          {isFollowed
            ? props.isFollowAccepted || !props.isPrivate
              ? "UnFollow"
              : "Follow Requested"
            : "Follow"}
        </Button>
      </Grid>
    </Grid>
  );
}
