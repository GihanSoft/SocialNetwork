import {
  mdiCommentOutline,
  mdiDelete,
  mdiDotsVertical,
  mdiStar,
  mdiStarOutline,
} from "@mdi/js";
import { Icon } from "@mdi/react";
import React from "react";

import Avatar from "@mui/material/Avatar";
import Button from "@mui/material/Button";
import ButtonBase from "@mui/material/ButtonBase";
import Card from "@mui/material/Card";
import CardActionArea from "@mui/material/CardActionArea";
import CardActions from "@mui/material/CardActions";
import CardContent from "@mui/material/CardContent";
import CardHeader from "@mui/material/CardHeader";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogContentText from "@mui/material/DialogContentText";
import DialogTitle from "@mui/material/DialogTitle";
import IconButton from "@mui/material/IconButton";
import ListItemIcon from "@mui/material/ListItemIcon";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import { useTheme } from "@mui/material/styles";
import Typography from "@mui/material/Typography";
import useMediaQuery from "@mui/material/useMediaQuery";
import { NavLink, useLocation, useNavigate } from "react-router";
import { RequestManager } from "../Services/RequestManager";

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
  const navigate = useNavigate();
  const location = useLocation();
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

  return (
    <Card
      sx={(theme) => ({
        margin: theme.spacing(1.5),
        boxShadow: "0 0 0 0 transparent",
      })}
    >
      <CardHeader
        sx={{ paddingTop: 1, paddingBottom: 1 }}
        avatar={
          <Avatar
            sx={{ fontSize: "20pt", width: 60, height: 60 }}
            src={`/api/user/avatar/${props.sender}`}
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
                justifyContent: "start",
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
            <Menu open={isMenuOpen} anchorEl={anchorEl}>
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
            <Dialog fullScreen={isMobile} open={isDialogOpen}>
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
                    var reqManager = new RequestManager();
                    reqManager.Send(
                      `/api/post/delete/${props.id}`,
                      "DELETE",
                      undefined,
                      undefined,
                      (xhr) => {
                        if (xhr.status === 200) {
                          handleClose();
                          setIsDialogOpen(false);
                          navigate(location, { replace: true });
                        }
                      }
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
          navigate("/Post/View/" + props.id);
        }}
      >
        <CardContent
          sx={(theme) => ({
            borderWidth: "0.5px 0",
            borderColor: theme.palette.grey[700],
            paddingTop: 2,
            paddingBottom: 2,
          })}
        >
          <Typography variant="body1" color="textSecondary" display="block">
            {props.text}
          </Typography>
        </CardContent>
      </CardActionArea>
      <CardActions
        sx={{
          paddingTop: 1,
          paddingBottom: 1,
        }}
      >
        <IconButton
          onClick={() => {
            let reqManager = new RequestManager();
            reqManager.Post(
              `/api/Post/${liked ? "Remove" : ""}Like`,
              (xhr) => {
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
            style={{ fill: "gold" }}
            path={liked ? mdiStar : mdiStarOutline}
            size={1}
          />
        </IconButton>
        <Typography variant="body1">{likeCount}</Typography>
        <span style={{ marginLeft: 20 }}></span>
        <IconButton>
          <Icon path={mdiCommentOutline} size={1} />
        </IconButton>
        <Typography variant="body1">{props.commentCount}</Typography>
      </CardActions>
    </Card>
  );
}
