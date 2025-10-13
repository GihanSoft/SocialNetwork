import Icon from "@mdi/react";
import { mdiMenu, mdiPlus, mdiLogin } from "@mdi/js";
import HideOnScroll from "./HideOnScroll";
import AuthorizeService from "../Services/AuthorizeService";
import MyDrawer from "./Drawer";
import authService from "../Services/AuthorizeService";
import { NavLink, Outlet, useNavigate } from "react-router";
import { useState } from "react";
import AppBar from "@mui/material/AppBar";
import Toolbar from "@mui/material/Toolbar";
import IconButton from "@mui/material/IconButton";
import Drawer from "@mui/material/Drawer";
import Fab from "@mui/material/Fab";
import Avatar from "@mui/material/Avatar";
import Stack from "@mui/material/Stack";
import { useTheme } from "@mui/material/styles";
import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import Button from "@mui/material/Button";

const drawerWidth = 256;

export default function Layout() {
  const navigate = useNavigate();
  const [isMobileDrawerOpen, setIsMobileDrawerOpen] = useState(false);
  const theme = useTheme();

  const handleDrawerToggle = () => {
    setIsMobileDrawerOpen(!isMobileDrawerOpen);
  };

  const routeTo = (url: string) => {
    navigate(url);
    setIsMobileDrawerOpen(false);
  };
  let username: string = authService.isSigned() || "";

  return (
    <Stack direction="column" sx={{ flex: "auto" }}>
      <HideOnScroll direction="down">
        <AppBar position="static">
          <Toolbar>
            <IconButton
              onClick={handleDrawerToggle}
              sx={{ display: { md: "none" } }}
            >
              <Icon
                path={mdiMenu}
                size={1.25}
                style={{
                  color: theme.palette.common.white,
                  fill: theme.palette.common.white,
                }}
              />
            </IconButton>
            <Button component={NavLink} to="/">
              G Social Network
            </Button>
            <Box flex="auto"></Box>
            {AuthorizeService.isSigned() ? (
              <IconButton component={NavLink} to={`/user/${username}`}>
                <Avatar
                  src={`api/user/avatar/${username}`}
                  alt={username[0].toUpperCase()}
                />
              </IconButton>
            ) : (
              <Button
                color="inherit"
                onClick={() => routeTo("/SignIn")}
                endIcon={<Icon path={mdiLogin} size={1.25} />}
              >
                SignIn
              </Button>
            )}
          </Toolbar>
        </AppBar>
      </HideOnScroll>
      <Container>
        <Outlet />
      </Container>

      {AuthorizeService.isSigned() && (
        <HideOnScroll direction="up">
          <Fab
            color="primary"
            onClick={() => {
              navigate("/post/new");
            }}
            sx={(theme) => ({
              position: "fixed",
              bottom: theme.spacing(1),
              insetInlineEnd: theme.spacing(1),
            })}
          >
            <Icon path={mdiPlus} size={1.25} />
          </Fab>
        </HideOnScroll>
      )}

      <Drawer
        sx={
          {
            // display: { xs: "flex", sm: "none" },
          }
        }
        variant="temporary"
        open={isMobileDrawerOpen}
        onClose={handleDrawerToggle}
      >
        <MyDrawer isMobileDrawerOpenSetter={setIsMobileDrawerOpen} />
      </Drawer>
    </Stack>
  );

  return (
    <Stack direction="column" sx={{ flex: "auto" }}>
      <Box
        component="nav"
        sx={(theme) => ({
          [theme.breakpoints.up("md")]: {
            width: drawerWidth,
            flexShrink: 0,
          },
        })}
      >
        {/* The implementation can be swapped with js to avoid SEO duplication of links. */}
        <Drawer
          sx={(theme) => ({
            display: { xs: "flex", sm: "none" },
          })}
          variant="temporary"
          open={isMobileDrawerOpen}
          onClose={handleDrawerToggle}
          ModalProps={{
            keepMounted: true, // Better open performance on mobile.
          }}
        >
          <MyDrawer isMobileDrawerOpenSetter={setIsMobileDrawerOpen} />
        </Drawer>
        <Drawer
          sx={(theme) => ({
            display: { xs: "flex", sm: "none" },
          })}
          variant="permanent"
          open
        >
          <MyDrawer isMobileDrawerOpenSetter={setIsMobileDrawerOpen} />
        </Drawer>
      </Box>
    </Stack>
  );
}
