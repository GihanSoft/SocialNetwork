import React from "react";
import { useHistory } from "react-router";
import useConfig from "../Services/Config";
import Post, { PostProps } from "../Components/Post";
import PostList from "../Components/PostList";
import { CircularProgress, IconButton } from "@material-ui/core";
import { RequestManager } from "../Services/RequestManager";
import config from "../Services/Config";
import { mdiSend } from "@mdi/js";
import Icon from "@mdi/react";

let gSetPost: (post: PostProps) => void;

async function getPost(postId: number): Promise<PostProps> {
    let config = useConfig;
    let response = await fetch(`${config.apiBase}/Post/View/${postId}`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("AuthJWT")}`
        }
    });
    let postProp = await response.json();
    return postProp;
}

export default function ViewPost() {
    let history = useHistory();
    let postId = parseInt(history.location.pathname.split("/").pop() || "0");
    getPost(postId).then(p => gSetPost(p));
    return <ViewPostInner />;
}

function SendComment() {
    let [charCount, setCharCount] = React.useState(0);
    let history = useHistory();
    let [text, setText] = React.useState<string>("");
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
                    marginRight: 12
                }}
                name="text"
                onChange={e => {
                    if (e.currentTarget.value.length > 300)
                        e.currentTarget.value = e.currentTarget.value.substr(
                            0,
                            300
                        );
                    setCharCount(e.currentTarget.value.length / 3);
                    setText(e.currentTarget.value);
                }}
                value={text}
            />
            <CircularProgress
                style={{
                    position: "relative",
                    top: -49,
                    left: "calc(100% - 57px)"
                }}
                value={charCount}
                color="primary"
                variant="static"
            />
            <IconButton
                style={{
                    position: "relative",
                    top: -66,
                    left: "calc(100% - 100px)"
                }}
                onClick={() => {
                    let reqManager = new RequestManager(config.apiBase);
                    reqManager.Post(
                        `/Post/SendComment/${history.location.pathname
                            .split("/")
                            .pop() || "0"}`,
                        xhr => {
                            if (xhr.status === 200) {
                                history.push(history.location.pathname);
                            }
                        },
                        {
                            text: (document.getElementsByName(
                                "text"
                            )[0] as HTMLTextAreaElement).value
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
    let [postProp, setPost] = React.useState<PostProps>();
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
