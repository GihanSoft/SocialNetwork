import React from "react";
import { RequestManager } from "../Services/RequestManager";
import PostList from "../Components/PostList";
import UserList from "../Components/UserList";
import { Link, NavLink, useLocation, useNavigate } from "react-router";
import Icon from "@mdi/react";
import { mdiCamera, mdiAccountEdit, mdiLockOutline } from "@mdi/js";
import authService from "../Services/AuthorizeService";
import Typography from "@mui/material/Typography";
import Box from "@mui/material/Box";
import Avatar from "@mui/material/Avatar";
import IconButton from "@mui/material/IconButton";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import ButtonBase from "@mui/material/ButtonBase";

interface UserData {
  userName: string;
  postsUrl: string;
  isFollowed: boolean;
  isFollowAccepted: boolean;
  isFollowRequested: boolean;
  isPrivate: boolean;
  followersCount: number;
  followingsCount: number;
}

let gSetUserData: React.Dispatch<React.SetStateAction<UserData | undefined>>;
let gSetAvatarUrl: React.Dispatch<React.SetStateAction<string | undefined>>;

export default function ViewUser() {
  const location = useLocation();
  const navigate = useNavigate();
  const pathParts = location.pathname.split("/").filter((v) => v !== "");
  if (pathParts.length === 3) {
    const lastPart = pathParts.pop();
    if (lastPart !== "followers" && lastPart !== "followings") navigate("/404");
    const username = pathParts.pop();
    return (
      <>
        <Typography
          variant="h6"
          style={{ display: "flex", justifyContent: "center" }}
        >{`${username} ${lastPart}`}</Typography>
        <UserList
          url={`api/User/List?username=${username}&${
            lastPart?.toLowerCase() === "followers"
              ? "follower=true"
              : "following=true"
          }`}
        />
      </>
    );
  }
  const username = pathParts.pop();
  let reqManager = new RequestManager();
  reqManager.Post("/user/" + username, (xhr) => {
    if (xhr.status === 200) {
      let userData = JSON.parse(xhr.responseText);
      gSetUserData(userData);
      gSetAvatarUrl(`api/user/avatar/${userData?.userName}`);
    }
  });
  return <ViewUserInternal />;
}

function ViewUserInternal() {
  let userData: UserData | undefined;
  [userData, gSetUserData] = React.useState<UserData>();
  let [avatarUrl, setAvatarUrl] = React.useState<string>();
  gSetAvatarUrl = setAvatarUrl;
  const toggleFollow = (username: string) => {
    let reqMng = new RequestManager();
    let action = (userData || {}).isFollowed ? "/UnFollow/" : "/Follow/";
    reqMng.Post("/User" + action + username, (xhr) => {
      if (xhr.status === 200)
        if (userData) {
          userData = {
            userName: userData.userName,
            postsUrl: userData.postsUrl,
            followersCount:
              userData.isPrivate &&
              (!userData.isFollowed || !userData.isFollowAccepted)
                ? userData.followersCount
                : userData.followersCount + (userData.isFollowed ? -1 : 1),
            followingsCount: userData.followingsCount,
            isFollowed: !userData.isFollowed,
            isFollowAccepted: !userData.isPrivate,
            isPrivate: userData.isPrivate,
            isFollowRequested: userData.isFollowRequested,
          };
          gSetUserData(userData);
        }
    });
  };
  return (
    <>
      <Box sx={{ height: "200px", backgroundColor: "#f5f5ff" }}></Box>

      <div
        style={{
          textDecoration: "none",
          display: "flex",
          flexDirection: "row",
          position: "relative",
          top: -25,
          marginLeft: 25,
        }}
      >
        <Link
          style={{ textDecoration: "none" }}
          to={`api/user/avatar/${userData?.userName}`}
        >
          <Avatar
            sx={(theme) => ({
              position: "relative",
              fontSize: "32pt",
              width: 100,
              height: 100,
              [theme.breakpoints.down("xs")]: {
                width: 75,
                height: 75,
              },
            })}
            src={avatarUrl}
            alt={userData ? userData.userName[0].toUpperCase() : ""}
          ></Avatar>
          {userData?.isPrivate && !userData.isFollowAccepted ? (
            <Icon
              style={{
                position: "relative",
                top: -100,
              }}
              path={mdiLockOutline}
            />
          ) : (
            ""
          )}
        </Link>
        <div
          style={{
            height: 25,
            width: 75,
            position: "absolute",
            bottom: 0,
          }}
          onMouseEnter={() => {
            if (authService.isSigned() === userData?.userName) {
              let element = document.getElementsByClassName("cameraP")[0];
              element.classList.remove("remove");
              element = document.getElementsByClassName("camera")[0];
              element.classList.remove("remove");
            }
          }}
        ></div>
        <input
          style={{ display: "none" }}
          accept="image/*"
          id="upload"
          type="file"
          onChange={() => {
            let upload: any = document.getElementById("upload");
            if (upload?.files?.length > 0) {
              let data = new FormData();
              data.append("file", upload.files[0]);
              fetch(`api/user/SetAvatar/${userData?.userName}`, {
                method: "POST",
                headers: {
                  // "Content-Type": "image/*",
                  Authorization: `Bearer ${localStorage.getItem("AuthJWT")}`,
                },
                body: data,
              }).then((r) => {
                if (r.status === 200) setAvatarUrl(avatarUrl + "");
              });
            }
          }}
        />
        {authService.isSigned() === userData?.userName ? (
          <label
            htmlFor="upload"
            className="cameraP remove"
            style={{
              position: "relative",
              width: 100,
              height: 100,
              left: -100,
            }}
          >
            <ButtonBase
              component="span"
              onMouseLeave={() => {
                if (authService.isSigned() === userData?.userName) {
                  let element = document.getElementsByClassName("cameraP")[0];
                  element.classList.add("remove");
                  element = document.getElementsByClassName("camera")[0];
                  element.classList.add("remove");
                }
              }}
            >
              <Icon
                path={mdiCamera}
                className="camera remove"
                style={{
                  width: 100,
                  height: 100,
                  paddingTop: 70,
                  background: "rgba(200,200,255, 0.5)",
                  position: "relative",
                  borderRadius: 50,
                  backgroundClip: "content-box",
                  transition: "0.5s",
                }}
              />
            </ButtonBase>
          </label>
        ) : (
          ""
        )}
        {authService.isSigned() === userData?.userName ? (
          <NavLink to="/EditUser">
            <IconButton
              sx={{ position: "absolute", right: "20px", margin: "auto" }}
            >
              <Icon path={mdiAccountEdit} style={{ width: 75, height: 75 }} />
            </IconButton>
          </NavLink>
        ) : (
          ""
        )}
      </div>
      <Grid container paddingInline={2}>
        <Grid size={{ xs: 12, sm: 3 }}>
          <Typography variant="h5" padding={0.5} textAlign="center">
            {userData && userData.userName}
          </Typography>
        </Grid>
        <Grid size={{ xs: 6, sm: 3 }}>
          <NavLink
            style={{ textDecoration: "none" }}
            to={
              !userData?.isPrivate || userData.isFollowAccepted
                ? `/user/${userData?.userName}/followers`
                : "#"
            }
          >
            <ButtonBase style={{ width: "100%" }}>
              <Typography variant="body1" padding={1} textAlign="center">
                followers: {userData && userData.followersCount}
              </Typography>
            </ButtonBase>
          </NavLink>
        </Grid>
        <Grid size={{ xs: 6, sm: 3 }}>
          <NavLink
            style={{ textDecoration: "none" }}
            to={
              !userData?.isPrivate || userData.isFollowAccepted
                ? `/user/${userData?.userName}/followings`
                : "#"
            }
          >
            <ButtonBase style={{ width: "100%" }}>
              <Typography variant="body1" padding={1} textAlign="center">
                followings: {userData && userData.followingsCount}
              </Typography>
            </ButtonBase>
          </NavLink>
        </Grid>
        {authService.isSigned() !== userData?.userName ? (
          <Grid size={{ xs: 12, sm: 3 }}>
            <Button
              color="primary"
              sx={(theme) => ({
                width: "100%",
                paddingLeft: theme.spacing(2),
                paddingRight: theme.spacing(2),
              })}
              onClick={() => {
                if (userData) toggleFollow(userData.userName);
              }}
              variant={
                userData && userData.isFollowed ? "contained" : "outlined"
              }
            >
              {userData && userData.isFollowed
                ? userData.isFollowAccepted
                  ? "UnFollow"
                  : "Follow Requested"
                : "Follow"}
            </Button>
          </Grid>
        ) : (
          ""
        )}
      </Grid>
      {!userData?.isPrivate || userData.isFollowAccepted ? (
        <PostList url={userData?.postsUrl || ""} />
      ) : (
        ""
      )}
    </>
  );
}
