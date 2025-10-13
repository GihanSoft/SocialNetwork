import { useState } from "react";
import { RequestManager } from "../Services/RequestManager";
import { useNavigate } from "react-router";
import Button from "@mui/material/Button";
import CircularProgress from "@mui/material/CircularProgress";
import Stack from "@mui/material/Stack";
import TextareaAutosize from "@mui/material/TextareaAutosize";
import { useTheme } from "@mui/material/styles";
import Box from "@mui/material/Box";

export default function SendPost() {
  const theme = useTheme();
  let [charCount, setCharCount] = useState(0);
  const navigate = useNavigate();
  return (
    <Stack direction="column" gap={1}>
      <TextareaAutosize
        style={{
          flex: "auto",
        }}
        name="text"
        onChange={(e) => {
          if (e.currentTarget.value.length > 300)
            e.currentTarget.value = e.currentTarget.value.substr(0, 300);
          setCharCount(e.currentTarget.value.length / 3);
        }}
      />
      <Stack direction="row" gap={1}>
        <Box flex="auto" />
        <CircularProgress
          value={charCount}
          color="primary"
          variant="determinate"
        />
        <Button
          variant="contained"
          color="primary"
          sx={(theme) => ({
            minWidth: "100px",
            marginRight: theme.spacing(1),
          })}
          onClick={() => {
            let reqManager = new RequestManager();
            reqManager.Post(
              "/api/post/send",
              (xhr) => {
                if (xhr.status === 200) {
                  navigate("/");
                }
              },
              {
                text: (
                  document.getElementsByName("text")[0] as HTMLTextAreaElement
                ).value,
              }
            );
          }}
        >
          Send
        </Button>
      </Stack>
    </Stack>
  );
}
