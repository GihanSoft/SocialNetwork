import React from "react";
import { useHistory } from "react-router";
import { RequestManager } from "../Services/RequestManager";
import Config from "../Services/Config";
import {
    makeStyles,
    Theme,
    createStyles,
    Avatar,
    Box,
    Typography,
    Grid,
    Button,
    Link,
    ButtonBase,
    IconButton
} from "@material-ui/core";
import PostList from "../Components/PostList";
import UserList from "../Components/UserList";
import { NavLink } from "react-router-dom";
import config from "../Services/Config";
import Icon from "@mdi/react";
import { mdiCamera, mdiAccountEdit } from "@mdi/js";
import authService from "../Services/AuthorizeService";

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        wall: {
            height: "200px",
            backgroundColor: "#f5f5ff"
        },
        avatarParent: {
            display: "flex",
            flexDirection: "row",
            position: "relative",
            top: -25,
            marginLeft: 25
            // width: 200,
            // [theme.breakpoints.down("xs")]: {
            //     width: 150
            // }
        },
        hugeAvatar: {
            position: "relative",
            fontSize: "32pt",
            width: 100,
            height: 100,
            [theme.breakpoints.down("xs")]: {
                width: 75,
                height: 75
            }
        },
        cameraP: {
            position: "relative",
            width: 100,
            height: 100,
            left: -100,
            // paddingTop: 70,
            [theme.breakpoints.down("xs")]: {
                width: 75,
                height: 75,
                left: -75
                // paddingTop: 50
            }
        },
        camera: {
            width: 100,
            height: 100,
            // left: -100,
            paddingTop: 70,
            background: "rgba(200,200,255, 0.5)",
            position: "relative",
            borderRadius: 50,
            backgroundClip: "content-box",
            transition: "0.5s",
            [theme.breakpoints.down("xs")]: {
                width: 75,
                height: 75,
                // left: -75,
                paddingTop: 50
            }
        },
        dataGrid: {
            paddingLeft: theme.spacing(2),
            paddingRight: theme.spacing(2)
        },
        gridItem: {
            padding: theme.spacing(1),
            textAlign: "center"
        },
        gridBigItem: {
            padding: theme.spacing(0.5),
            textAlign: "center"
        },
        btnFollow: {
            width: "100%",
            paddingLeft: theme.spacing(2),
            paddingRight: theme.spacing(2)
        },
        remove: {
            height: 0,
            padding: 0
        },
        EditBtn: {
            position: "absolute",
            right: "20px",
            margin: "auto"
            // width: 75,
            // height: 75,
            // [theme.breakpoints.down("xs")]: {
            //     width: 50,
            //     height: 50
            // }
        },
        EditBtnIcon: {
            width: 75,
            height: 75,
            [theme.breakpoints.down("xs")]: {
                width: 50,
                height: 50
            }
        }
    })
);

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
    const config = Config;
    const history = useHistory();
    const pathParts = history.location.pathname
        .split("/")
        .filter(v => v !== "");
    if (pathParts.length === 3) {
        const lastPart = pathParts.pop();
        if (lastPart !== "followers" && lastPart !== "followings")
            history.push("/404");
        const username = pathParts.pop();
        return (
            <>
                <Typography
                    variant="h6"
                    style={{ display: "flex", justifyContent: "center" }}
                >{`${username} ${lastPart}`}</Typography>
                <UserList
                    url={`${config.apiBase}/User/List?username=${username}&${
                        lastPart?.toLowerCase() === "followers"
                            ? "follower=true"
                            : "following=true"
                    }`}
                />
            </>
        );
    }
    const username = pathParts.pop();
    let reqManager = new RequestManager(config.apiBase);
    reqManager.Post("/user/" + username, xhr => {
        if (xhr.status === 200) {
            let userData = JSON.parse(xhr.responseText);
            gSetUserData(userData);
            gSetAvatarUrl(
                `${config.apiBase}/user/avatar/${userData?.userName}`
            );
        }
    });
    return <ViewUserInternal />;
}

function ViewUserInternal() {
    let userData: UserData | undefined;
    [userData, gSetUserData] = React.useState<UserData>();
    let [avatarUrl, setAvatarUrl] = React.useState<string>();
    gSetAvatarUrl = setAvatarUrl;
    const classes = useStyles();
    const toggleFollow = (username: string) => {
        const config = Config;
        let reqMng = new RequestManager(config.apiBase);
        let action = (userData || {}).isFollowed ? "/UnFollow/" : "/Follow/";
        reqMng.Post("/User" + action + username, xhr => {
            if (xhr.status === 200)
                if (userData) {
                    userData = {
                        userName: userData.userName,
                        postsUrl: userData.postsUrl,
                        followersCount:
                            userData.isPrivate &&
                            (!userData.isFollowed || !userData.isFollowAccepted)
                                ? userData.followersCount
                                : userData.followersCount +
                                  (userData.isFollowed ? -1 : 1),
                        followingsCount: userData.followingsCount,
                        isFollowed: !userData.isFollowed,
                        isFollowAccepted: !userData.isPrivate,
                        isPrivate: userData.isPrivate,
                        isFollowRequested: userData.isFollowRequested
                    };
                    gSetUserData(userData);
                }
        });
    };
    return (
        <>
            <Box className={classes.wall}></Box>

            <div
                className={classes.avatarParent}
                style={{ textDecoration: "none" }}
            >
                <Link
                    style={{ textDecoration: "none" }}
                    href={`${config.apiBase}/user/avatar/${userData?.userName}`}
                >
                    <Avatar
                        className={classes.hugeAvatar}
                        src={avatarUrl}
                        alt={userData ? userData.userName[0].toUpperCase() : ""}
                    ></Avatar>
                </Link>
                <div
                    style={{
                        height: 25,
                        width: 75,
                        position: "absolute",
                        bottom: 0
                    }}
                    onMouseEnter={() => {
                        if (authService.isSigned() === userData?.userName) {
                            let element = document.getElementsByClassName(
                                classes.cameraP
                            )[0];
                            element.classList.remove(classes.remove);
                            element = document.getElementsByClassName(
                                classes.camera
                            )[0];
                            element.classList.remove(classes.remove);
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
                            fetch(
                                `${config.apiBase}/user/SetAvatar/${userData?.userName}`,
                                {
                                    method: "POST",
                                    headers: {
                                        // "Content-Type": "image/*",
                                        Authorization: `Bearer ${localStorage.getItem(
                                            "AuthJWT"
                                        )}`
                                    },
                                    body: data
                                }
                            ).then(r => {
                                if (r.status === 200)
                                    setAvatarUrl(avatarUrl + "");
                            });
                        }
                    }}
                />
                {authService.isSigned() === userData?.userName ? (
                    <label
                        htmlFor="upload"
                        className={classes.cameraP + " " + classes.remove}
                    >
                        <ButtonBase
                            component="span"
                            onMouseLeave={() => {
                                if (
                                    authService.isSigned() ===
                                    userData?.userName
                                ) {
                                    let element = document.getElementsByClassName(
                                        classes.cameraP
                                    )[0];
                                    element.classList.add(classes.remove);
                                    element = document.getElementsByClassName(
                                        classes.camera
                                    )[0];
                                    element.classList.add(classes.remove);
                                }
                            }}
                        >
                            <Icon
                                path={mdiCamera}
                                className={
                                    classes.camera + " " + classes.remove
                                }
                            />
                        </ButtonBase>
                    </label>
                ) : (
                    ""
                )}
                {authService.isSigned() === userData?.userName ? (
                    <NavLink to="/EditUser">
                        <IconButton className={classes.EditBtn}>
                            <Icon
                                path={mdiAccountEdit}
                                className={classes.EditBtnIcon}
                            />
                        </IconButton>
                    </NavLink>
                ) : (
                    ""
                )}
            </div>
            <Grid container className={classes.dataGrid} justify="space-evenly">
                <Grid item xs={12} sm={3}>
                    <Typography variant="h5" className={classes.gridBigItem}>
                        {userData && userData.userName}
                    </Typography>
                </Grid>
                <Grid item xs={6} sm={3}>
                    <NavLink
                        style={{ textDecoration: "none" }}
                        to={`/user/${userData?.userName}/followers`}
                    >
                        <ButtonBase style={{ width: "100%" }}>
                            <Typography
                                variant="body1"
                                className={classes.gridItem}
                            >
                                followers: {userData && userData.followersCount}
                            </Typography>
                        </ButtonBase>
                    </NavLink>
                </Grid>
                <Grid item xs={6} sm={3}>
                    <NavLink
                        style={{ textDecoration: "none" }}
                        to={`/user/${userData?.userName}/followings`}
                    >
                        <ButtonBase style={{ width: "100%" }}>
                            <Typography
                                variant="body1"
                                className={classes.gridItem}
                            >
                                followings:{" "}
                                {userData && userData.followingsCount}
                            </Typography>
                        </ButtonBase>
                    </NavLink>
                </Grid>
                {authService.isSigned() !== userData?.userName ? (
                    <Grid item xs={12} sm={3}>
                        <Button
                            color="primary"
                            className={classes.btnFollow}
                            onClick={() => {
                                if (userData) toggleFollow(userData.userName);
                            }}
                            variant={
                                userData && userData.isFollowed
                                    ? "contained"
                                    : "outlined"
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
            <PostList url={(userData || { postsUrl: "" }).postsUrl || ""} />
        </>
    );
}
