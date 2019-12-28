import React from "react";
import {
    Divider,
    List,
    ListItem,
    ListItemIcon,
    ListItemText,
    Theme,
    makeStyles,
    createStyles
} from "@material-ui/core";
import Icon from "@mdi/react";
import { mdiHome, mdiMagnify, mdiPlus, mdiLogout } from "@mdi/js";
import AuthorizeService from "../Services/AuthorizeService";
import { useHistory } from "react-router";

const drawerWidth = 240;

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        root: {
            display: "flex"
        },
        lastNI: {
            flexGrow: 1,
            width: "calc(100% - 18px)",
            height: "calc(100% - 18px)",
            margin: 10
        },
        LB: {
            margin: "10px 0"
        },
        drawer: {
            [theme.breakpoints.up("md")]: {
                width: drawerWidth,
                flexShrink: 0
            }
        },
        appBar: {
            marginLeft: drawerWidth,
            zIndex: theme.zIndex.modal + 1
        },
        navbar: {
            color: "inherit",
            width: "fill"
        },
        menuButton: {
            margin: 5,
            marginRight: theme.spacing(2),
            [theme.breakpoints.up("md")]: {
                display: "none"
            }
        },
        toolbar: theme.mixins.toolbar,
        drawerPaper: {
            width: drawerWidth
        },
        content: {
            flexGrow: 1,
            padding: theme.spacing(0)
        },
        fab: {
            position: "fixed",
            right: "0",
            bottom: "0",
            margin: theme.spacing(1)
        },
        icon: {
            color: theme.palette.common.white,
            fill: theme.palette.common.white
        },
        link: {
            color: theme.palette.common.white,
            textDecoration: "none"
        }
    })
);

export default function Drawer(props: {
    isMobileDrawerOpenSetter: React.Dispatch<React.SetStateAction<boolean>>;
}) {
    const history = useHistory();
    const routeTo = (url: string) => {
        history.push(url);
        props.isMobileDrawerOpenSetter(false);
    };
    const classes = useStyles();

    return (
        <>
            <div className={classes.toolbar} />
            <Divider />
            <List>
                <ListItem
                    button
                    onClick={() => routeTo("/")}
                    selected={history.location.pathname === "/"}
                >
                    <ListItemIcon>
                        <Icon path={mdiHome} size={1.25} />
                    </ListItemIcon>
                    <ListItemText>Home</ListItemText>
                </ListItem>
                <ListItem
                    button
                    onClick={() => routeTo("/search")}
                    selected={history.location.pathname === "/search"}
                >
                    <ListItemIcon>
                        <Icon path={mdiMagnify} size={1.25} />
                    </ListItemIcon>
                    <ListItemText>Search</ListItemText>
                </ListItem>
                {AuthorizeService.isSigned() ? (
                    <ListItem
                        button
                        onClick={() => routeTo("/post/new")}
                        selected={history.location.pathname === "/post/new"}
                    >
                        <ListItemIcon>
                            <Icon path={mdiPlus} size={1.25} />
                        </ListItemIcon>
                        <ListItemText>New Post</ListItemText>
                    </ListItem>
                ) : (
                    ""
                )}
                {AuthorizeService.isSigned() ? (
                    <ListItem
                        button
                        onClick={() => routeTo("/SignOut")}
                        selected={history.location.pathname === "/SignOut"}
                    >
                        <ListItemIcon>
                            <Icon path={mdiLogout} size={1.25} />
                        </ListItemIcon>
                        <ListItemText>SignOut</ListItemText>
                    </ListItem>
                ) : (
                    ""
                )}
            </List>
        </>
    );
}
