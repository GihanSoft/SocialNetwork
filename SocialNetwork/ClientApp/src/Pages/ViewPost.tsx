import Post, { type PostProps } from "../Components/Post";
import PostList from "../Components/PostList";
import { RequestManager } from "../Services/RequestManager";
import { mdiSend } from "@mdi/js";
import Icon from "@mdi/react";
import { useState } from "react";
import CircularProgress from "@mui/material/CircularProgress";
import IconButton from "@mui/material/IconButton";
import { useLocation, useNavigate } from "react-router";

let gSetPost: (post: PostProps) => void;

async function getPost(postId: number): Promise<PostProps> {
  let response = await fetch(`/api/Post/View/${postId}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${localStorage.getItem("AuthJWT")}`,
    },
  });
  let postProp = await response.json();
  return postProp;
}

export default function ViewPost() {
  const location = useLocation();
  let postId = parseInt(location.pathname.split("/").pop() || "0");
  getPost(postId).then((p) => gSetPost(p));
  return <ViewPostInner />;
}

function SendComment() {
  let [charCount, setCharCount] = useState(0);
  const location = useLocation();
  const navigate = useNavigate();
  let [text, setText] = useState<string>("");
  return (
    <div style={{ height: "128px" }}>
      <textarea
        style={{
          minWidth: "calc(100% - 24px)",
          maxWidth: "calc(100% - 24px)",
          minHeight: "128px",
          maxHeight: "128px",
          paddingRight: "50px",
          scrollBehavior: "revert",
          marginLeft: 12,
          marginRight: 12,
        }}
        name="text"
        onChange={(e) => {
          if (e.currentTarget.value.length > 300)
            e.currentTarget.value = e.currentTarget.value.substr(0, 300);
          setCharCount(e.currentTarget.value.length / 3);
          setText(e.currentTarget.value);
        }}
        value={text}
      />
      <CircularProgress
        style={{
          position: "relative",
          top: -49,
          left: "calc(100% - 57px)",
        }}
        value={charCount}
        color="primary"
        variant="determinate"
      />
      <IconButton
        style={{
          position: "relative",
          top: -66,
          left: "calc(100% - 100px)",
        }}
        onClick={() => {
          let reqManager = new RequestManager();
          reqManager.Post(
            `/api/Post/SendComment/${location.pathname.split("/").pop() || "0"}`,
            (xhr) => {
              if (xhr.status === 200) {
                navigate(location.pathname);
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
        <Icon path={mdiSend} size={1} />
      </IconButton>
    </div>
  );
}

function ViewPostInner() {
  let [postProp, setPost] = useState<PostProps>();
  gSetPost = setPost;
  if (postProp === undefined) return <></>;
  return (
    <>
      <Post
        id={postProp.id}
        key={0}
        sender={postProp.sender}
        time={postProp.time}
        text={postProp.text}
        liked={postProp.liked}
        likesCount={postProp.likesCount}
        commentCount={postProp.commentCount}
      />
      <SendComment />
      <PostList url={`/API/Post/PostComments/${postProp.id}`} />
    </>
  );
}
