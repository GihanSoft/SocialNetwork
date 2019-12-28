import React from "react";
import {
    Theme,
    createStyles,
    makeStyles,
    Grid,
    ButtonBase,
    Button,
    Avatar,
    Typography
} from "@material-ui/core";
import { NavLink } from "react-router-dom";
import config from "../Services/Config";
import { RequestManager } from "../Services/RequestManager";

export interface UserProps {
    userName: string;
    isFollowed: boolean;
    isFollowRequested: boolean;
}

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        root: {
            width: "calc(100% - 10px)",
            margin: 5,
            [theme.breakpoints.up("sm")]: {
                width: 500,
                margin: "auto",
                marginTop: 5,
                marginBottom: 5
            }
        },
        button: {
            width: "100%",
            padding: 1.75
        },
        acceptBtn: {
            color: "green",
            border: "1px solid rgba(0, 128, 0, 0.5)",
            "&:hover": {
                color: "forestgreen",
                border: "1px solid rgba(34, 139, 34, 0.5)",
                backgroundColor: "rgba(0, 128, 0, 0.08)"
            }
        },
        userData: {
            width: "100%",
            justifyContent: "unset"
        },
        username: {
            fontSize: "16pt",
            paddingLeft: 25
        }
    })
);

export default function User(props: UserProps) {
    const classes = useStyles();
    let [isFollowed, setIsFollowed] = React.useState<boolean>(props.isFollowed);
    //let [isFollowRequested, setIsFollowRequested] = React.useState<boolean>(props.isFollowRequested);
    const toggleFollow = (username: string) => {
        let reqMng = new RequestManager(config.apiBase);
        let action = isFollowed ? "/UnFollow/" : "/Follow/";
        reqMng.Post("/User" + action + username, xhr => {
            if (xhr.status === 200) {
                setIsFollowed(!isFollowed);
            }
        });
    };
    return (
        <Grid container className={classes.root} spacing={0}>
            <Grid item xs={props.isFollowRequested ? 6 : 9}>
                <NavLink
                    to={`/user/${props.userName}`}
                    style={{ textDecoration: "none", color: "black" }}
                >
                    <ButtonBase className={classes.userData}>
                        <Avatar
                            src={`${config.apiBase}/user/avatar/${props.userName}`}
                            alt={props.userName[0].toUpperCase()}
                        />
                        <Typography className={classes.username}>
                            {props.userName}
                        </Typography>
                    </ButtonBase>
                </NavLink>
            </Grid>
            {props.isFollowRequested ? (
                <Grid item xs={3} className={classes.button}>
                    <Button
                        fullWidth
                        className={classes.acceptBtn}
                        variant="outlined"
                    >
                        Accept
                    </Button>
                </Grid>
            ) : (
                ""
            )}
            <Grid item xs={3} className={classes.button}>
                <Button
                    fullWidth
                    color="primary"
                    onClick={() => {
                        toggleFollow(props.userName);
                    }}
                    variant={isFollowed ? "contained" : "outlined"}
                >
                    {isFollowed ? "UnFollow" : "Follow"}
                </Button>
            </Grid>
        </Grid>
    );
}
