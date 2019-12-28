import React from "react";
import { useHistory } from "react-router";
import useConfig from "../Services/Config";
import Post, { PostProps } from "../Components/Post";

async function getPost(postId: number) {
    let config = useConfig;

    let response = await fetch(`${config.apiBase}/Post/View/${postId}`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("AuthJWT")}`
        }
    });
    let postProp: PostProps = await response.json();
    gSetInner((
        <Post
            id={postProp.id}
            key={0}
            sender={postProp.sender}
            time={postProp.time}
            text={postProp.text}
            likesCount={postProp.likesCount}
            liked={postProp.liked}
        />
    ));
}

export default function ViewPost() {
    let history = useHistory();
    let postId = parseInt(history.location.pathname.split("/").pop() || "0");
    getPost(postId);
    return <ViewPostInner />;
}

let gSetInner: React.Dispatch<React.SetStateAction<JSX.Element>>;

function ViewPostInner() {
    let [inner, setInner] = React.useState<JSX.Element>(<></>);
    gSetInner = setInner;
    return inner;
}
