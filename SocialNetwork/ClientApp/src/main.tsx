import { createTheme, CssBaseline, ThemeProvider } from "@mui/material";
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App.tsx";
import "./main.css";

const darkTheme = createTheme({
  colorSchemes: { dark: true, light: true },
  defaultColorScheme: "dark",
  typography: {
    fontFamily: "Vazirmatn",
  },
});

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ThemeProvider
      theme={darkTheme}
      defaultMode="system"
      disableTransitionOnChange
    >
      <CssBaseline enableColorScheme />
      {/* <AuthProvider> */}
      <App />
      {/* </AuthProvider> */}
    </ThemeProvider>
  </StrictMode>
);
