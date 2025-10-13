import Icon from "@mdi/react";
import { mdiHome, mdiMagnify, mdiPlus, mdiLogout } from "@mdi/js";
import AuthorizeService from "../Services/AuthorizeService";
import { useLocation, useNavigate } from "react-router";
import Divider from "@mui/material/Divider";
import List from "@mui/material/List";
import ListItemIcon from "@mui/material/ListItemIcon";
import ListItemText from "@mui/material/ListItemText";
import ListItemButton from "@mui/material/ListItemButton";
import Toolbar from "@mui/material/Toolbar";
import Box from "@mui/material/Box";

export default function Drawer(props: {
  isMobileDrawerOpenSetter: React.Dispatch<React.SetStateAction<boolean>>;
}) {
  const navigate = useNavigate();
  const location = useLocation();

  const routeTo = (url: string) => {
    navigate(url);
    props.isMobileDrawerOpenSetter(false);
  };

  return (
    <Box width={250}>
      <Toolbar />
      <Divider />
      <List>
        <ListItemButton
          onClick={() => routeTo("/")}
          selected={location.pathname === "/"}
        >
          <ListItemIcon>
            <Icon path={mdiHome} size={1.25} />
          </ListItemIcon>
          <ListItemText>Home</ListItemText>
        </ListItemButton>
        <ListItemButton
          onClick={() => routeTo("/search")}
          selected={location.pathname === "/search"}
        >
          <ListItemIcon>
            <Icon path={mdiMagnify} size={1.25} />
          </ListItemIcon>
          <ListItemText>Search</ListItemText>
        </ListItemButton>
        {AuthorizeService.isSigned() ? (
          <ListItemButton
            onClick={() => routeTo("/post/new")}
            selected={location.pathname === "/post/new"}
          >
            <ListItemIcon>
              <Icon path={mdiPlus} size={1.25} />
            </ListItemIcon>
            <ListItemText>New Post</ListItemText>
          </ListItemButton>
        ) : (
          ""
        )}
        {AuthorizeService.isSigned() ? (
          <ListItemButton
            onClick={() => routeTo("/SignOut")}
            selected={location.pathname === "/SignOut"}
          >
            <ListItemIcon>
              <Icon path={mdiLogout} size={1.25} />
            </ListItemIcon>
            <ListItemText>SignOut</ListItemText>
          </ListItemButton>
        ) : (
          ""
        )}
      </List>
    </Box>
  );
}
