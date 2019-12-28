import React from "react";
import { useHistory, NavLink } from "react-router-dom";
import {
    AppBar,
    CssBaseline,
    Drawer,
    Fab,
    Hidden,
    IconButton,
    Toolbar,
    Typography,
    ButtonBase,
    Button,
    Grid,
    Avatar
} from "@material-ui/core";
import {
    makeStyles,
    useTheme,
    Theme,
    createStyles
} from "@material-ui/core/styles";
import Icon from "@mdi/react";
import { mdiMenu, mdiPlus, mdiLogin } from "@mdi/js";
import HideOnScroll from "./HideOnScroll";
import AuthorizeService from "../Services/AuthorizeService";
import MyDrawer from "./Drawer";
import config from "../Services/Config";
import authService from "../Services/AuthorizeService";

const drawerWidth = 256;

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        root: {
            display: "flex"
        },
        lastNI: {
            flexGrow: 1,
            width: "calc(100% - 18px)",
            height: "calc(100% - 18px)",
            margin: 9,
            color: theme.palette.getContrastText(theme.palette.primary.main)
        },
        LB: {
            margin: "11px 0"
        },
        drawer: {
            [theme.breakpoints.up("md")]: {
                width: drawerWidth,
                flexShrink: 0
            }
        },
        appBar: {
            boxShadow: "0 0 0 0 transparent",
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
        toolbar: {
            minHeight: 64
        },
        drawerPaper: {
            width: drawerWidth
        },
        content: {
            marginRight: drawerWidth,
            flexGrow: 1,
            padding: theme.spacing(0),
            [theme.breakpoints.down("sm")]: {
                width: "75%",
                marginRight: 0
            }
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

interface ResponsiveDrawerProps {
    container?: Element;
    children: any;
}

export default function Layout(props: ResponsiveDrawerProps) {
    const history = useHistory();
    const { container } = props;
    const classes = useStyles();
    const theme = useTheme();
    const [isMobileDrawerOpen, setIsMobileDrawerOpen] = React.useState(false);

    const handleDrawerToggle = () => {
        setIsMobileDrawerOpen(!isMobileDrawerOpen);
    };

    const routeTo = (url: string) => {
        history.push(url);
        setIsMobileDrawerOpen(false);
    };
    let username: string = authService.isSigned() || "";

    return (
        <div className={classes.root}>
            <CssBaseline />
            <HideOnScroll direction="down">
                <AppBar position="fixed" className={classes.appBar}>
                    <Toolbar className={classes.navbar}>
                        <Grid container spacing={0}>
                            <Grid item xs={3} sm={2} lg={1}>
                                <IconButton
                                    aria-label="open drawer"
                                    edge="start"
                                    onClick={handleDrawerToggle}
                                    className={classes.menuButton}
                                >
                                    <Icon
                                        path={mdiMenu}
                                        size={1.25}
                                        className={classes.icon}
                                    />
                                </IconButton>
                            </Grid>
                            <Grid item xs={6} sm={8} lg={10}>
                                <NavLink
                                    to="/"
                                    style={{ textDecoration: "none" }}
                                >
                                    <ButtonBase
                                        className={classes.lastNI}
                                        // onClick={() => routeTo("/")}
                                    >
                                        <Typography variant="h6" noWrap>
                                            G Social Network
                                        </Typography>
                                    </ButtonBase>
                                </NavLink>
                            </Grid>
                            <Grid
                                item
                                xs={3}
                                sm={2}
                                lg={1}
                                style={{
                                    display: "flex",
                                    justifyContent: "flex-end"
                                }}
                            >
                                {AuthorizeService.isSigned() ? (
                                    <NavLink
                                        to={`/user/${username}`}
                                        style={{ textDecoration: "none" }}
                                    >
                                        <IconButton>
                                            <Avatar
                                                src={`${config.apiBase}/user/avatar/${username}`}
                                                alt={username[0].toUpperCase()}
                                            />
                                        </IconButton>
                                    </NavLink>
                                ) : (
                                    <Button
                                        className={classes.LB}
                                        onClick={() => routeTo("/SignIn")}
                                    >
                                        <Typography
                                            variant="body1"
                                            className={classes.icon}
                                        >
                                            SignIn
                                        </Typography>
                                        <Icon
                                            path={mdiLogin}
                                            size={1.25}
                                            className={classes.icon}
                                        />
                                    </Button>
                                )}
                            </Grid>
                        </Grid>
                    </Toolbar>
                </AppBar>
            </HideOnScroll>
            <nav className={classes.drawer}>
                {/* The implementation can be swapped with js to avoid SEO duplication of links. */}
                <Hidden smUp implementation="css">
                    <Drawer
                        container={container}
                        variant="temporary"
                        anchor={theme.direction === "rtl" ? "right" : "left"}
                        open={isMobileDrawerOpen}
                        onClose={handleDrawerToggle}
                        classes={{
                            paper: classes.drawerPaper
                        }}
                        ModalProps={{
                            keepMounted: true // Better open performance on mobile.
                        }}
                    >
                        <MyDrawer
                            isMobileDrawerOpenSetter={setIsMobileDrawerOpen}
                        />
                    </Drawer>
                </Hidden>
                <Hidden smDown implementation="css">
                    <Drawer
                        classes={{
                            paper: classes.drawerPaper
                        }}
                        variant="permanent"
                        open
                    >
                        <MyDrawer
                            isMobileDrawerOpenSetter={setIsMobileDrawerOpen}
                        />
                    </Drawer>
                </Hidden>
            </nav>
            <main className={classes.content}>
                <div className={classes.toolbar} />
                {props.children}
            </main>
            {AuthorizeService.isSigned() ? (
                <Hidden mdUp>
                    <HideOnScroll direction="up">
                        <Fab
                            className={classes.fab}
                            color="primary"
                            onClick={() => {
                                history.push("/post/new");
                            }}
                        >
                            <Icon
                                path={mdiPlus}
                                size={1.25}
                                className={classes.icon}
                            />
                        </Fab>
                    </HideOnScroll>
                </Hidden>
            ) : (
                ""
            )}
        </div>
    );
}
