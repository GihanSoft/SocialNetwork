import React from "react";
import {
    Avatar,
    Typography,
    Card,
    CardHeader,
    CardContent,
    IconButton,
    CardActions,
    MenuItem,
    Menu,
    ListItemIcon,
    useMediaQuery,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogContentText,
    DialogActions,
    Button,
    CardActionArea,
    ButtonBase
} from "@material-ui/core";
import {
    makeStyles,
    createStyles,
    Theme,
    useTheme
} from "@material-ui/core/styles";
import { Icon } from "@mdi/react";
import {
    mdiDotsVertical,
    mdiStar,
    mdiStarOutline,
    mdiDelete,
    mdiCommentOutline
} from "@mdi/js";
import { yellow, blueGrey } from "@material-ui/core/colors";
import { useHistory } from "react-router";
import { RequestManager } from "../Services/RequestManager";
import useConfig from "../Services/Config";
import { NavLink } from "react-router-dom";

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        card: {
            margin: theme.spacing(1.5),
            boxShadow: "0 0 0 0 transparent"
        },
        cardHead: {
            paddingTop: 8,
            paddingBottom: 8
        },
        cardFooter: {
            paddingTop: 4,
            paddingBottom: 4
        },
        cardBody: {
            borderWidth: "0.5px 0",
            borderColor: blueGrey[700],
            //border: 'solid'
            paddingTop: 0,
            paddingBottom: 0
        },
        bigAvatar: {
            fontSize: "20pt",
            width: 60,
            height: 60
        },
        likeIcon: {
            fill: yellow[700]
        }
    })
);

export interface PostProps {
    id: number;
    sender: string;
    text: string;
    time: string;
    liked?: boolean;
    likesCount: number;
    commentCount: number;
}

export default function Post(props: PostProps) {
    const classes = useStyles();
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));
    let [anchorEl, setAnchorEl] = React.useState<HTMLElement | null>(null);
    let isMenuOpen = Boolean(anchorEl);
    let [isDialogOpen, setIsDialogOpen] = React.useState(false);
    let [likeCount, setLikeCount] = React.useState(props.likesCount);
    let [liked, setLiked] = React.useState(props.liked);
    const toggleLiked = () => setLiked(!liked);
    const handleClick = (event: React.MouseEvent<HTMLElement>) => {
        setAnchorEl(event.currentTarget);
    };
    const handleClose = () => {
        setAnchorEl(null);
    };
    const history = useHistory();
    const config = useConfig;

    return (
        <Card className={classes.card}>
            <CardHeader
                className={classes.cardHead}
                avatar={
                    <Avatar
                        className={classes.bigAvatar}
                        src={`${config.apiBase}/user/avatar/${props.sender}`}
                        alt={props.sender[0].toUpperCase()}
                    ></Avatar>
                }
                title={
                    <NavLink
                        to={"/user/" + props.sender}
                        style={{ textDecoration: "none" }}
                    >
                        <ButtonBase
                            style={{
                                width: "100%",
                                display: "flex",
                                justifyContent: "start"
                            }}
                        >
                            <Typography variant="h6">{props.sender}</Typography>
                        </ButtonBase>
                    </NavLink>
                }
                subheader={props.time}
                action={
                    <div>
                        <IconButton onClick={handleClick}>
                            <Icon path={mdiDotsVertical} size={1} />
                        </IconButton>
                        <Menu
                            open={isMenuOpen}
                            anchorEl={anchorEl}
                            onBackdropClick={handleClose}
                        >
                            <MenuItem
                                onClick={() => {
                                    setIsDialogOpen(true);
                                }}
                            >
                                <ListItemIcon>
                                    <Icon path={mdiDelete} size={1} />
                                </ListItemIcon>
                                Delete
                            </MenuItem>
                            {/* <MenuItem onClick={handleClose}>
                                <ListItemIcon>
                                    <Icon path={mdiPencil} size={1} />
                                </ListItemIcon>
                                item2
                            </MenuItem> */}
                        </Menu>
                        <Dialog
                            fullScreen={isMobile}
                            open={isDialogOpen}
                            onBackdropClick={() => setIsDialogOpen(false)}
                        >
                            <DialogTitle>Warning</DialogTitle>
                            <DialogContent>
                                <DialogContentText>
                                    Are you sure to remove post?
                                </DialogContentText>
                            </DialogContent>
                            <DialogActions>
                                <Button
                                    color="primary"
                                    onClick={() => {
                                        var reqManager = new RequestManager(
                                            config.apiBase
                                        );
                                        reqManager.Post(
                                            "/post/delete",
                                            xhr => {
                                                if (xhr.status === 200) {
                                                    handleClose();
                                                    setIsDialogOpen(false);
                                                    let location =
                                                        history.location
                                                            .pathname;
                                                    history.replace(location);
                                                }
                                            },
                                            props.id
                                        );
                                    }}
                                >
                                    Yes
                                </Button>
                                <Button
                                    color="primary"
                                    autoFocus
                                    onClick={() => setIsDialogOpen(false)}
                                >
                                    No
                                </Button>
                            </DialogActions>
                        </Dialog>
                    </div>
                }
            ></CardHeader>
            <CardActionArea
                onClick={() => {
                    history.push("/Post/View/" + props.id);
                }}
            >
                <CardContent className={classes.cardBody}>
                    <Typography
                        variant="body1"
                        color="textSecondary"
                        display="block"
                    >
                        {props.text}
                    </Typography>
                </CardContent>
            </CardActionArea>
            <CardActions className={classes.cardFooter}>
                <IconButton
                    onClick={() => {
                        let reqManager = new RequestManager(config.apiBase);
                        reqManager.Post(
                            `/Post/${liked ? "Remove" : ""}Like`,
                            xhr => {
                                if (xhr.status === 200) {
                                    setLikeCount(parseInt(xhr.responseText));
                                    toggleLiked();
                                }
                            },
                            props.id
                        );
                    }}
                >
                    <Icon
                        className={classes.likeIcon}
                        path={liked ? mdiStar : mdiStarOutline}
                        size={1}
                    />
                </IconButton>
                <Typography variant="body1">{likeCount}</Typography>
                <span style={{marginLeft:20}}></span>
                <IconButton>
                    <Icon path={mdiCommentOutline} size={1} />
                </IconButton>
                <Typography variant="body1">{props.commentCount}</Typography>
            </CardActions>
        </Card>
    );
}
