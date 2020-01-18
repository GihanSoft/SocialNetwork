import React from "react";
import Post, { PostProps } from "./Post";
import useConfig from "../Services/Config";

interface PostListProps {
    url: string;
}

const getPosts = async (
    url: string | undefined,
    lastIp: number | undefined,
    max: number,
    getElders: boolean
): Promise<PostProps[]> => {
    let config = useConfig;
    let data = {
        LastGuttedId: lastIp,
        MaxCountToGet: max,
        TowardOlds: getElders
    };
    url = `${config.apiBase.replace("/API", "")}${url || "/API/Post/View"}`;
    let response = await fetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("AuthJWT")}`
        },
        body: JSON.stringify(data)
    });
    if (response.status === 401) {
        
    }
    if (!response.ok) return [];
    let posts = await response.json();
    return posts;
};

const useData = async (url: string, callback: (props: PostProps[]) => void) => {
    callback(await getPosts(url, undefined, 10, true));
};

export default function PostList(props: PostListProps) {
    useData(props.url, props => {
        gSetPostProps(props);
    });
    return <PostListViewer url={props.url} />;
}

let gSetPostProps: React.Dispatch<React.SetStateAction<
    PostProps[] | undefined
>>;

function PostListViewer(props: PostListProps) {
    let [posts, setPostProps] = React.useState<PostProps[] | undefined>();
    gSetPostProps = setPostProps;
    window.onscroll = async (ev: Event) => {
        if (window.scrollY === 0) {
            let upperPost = (posts || [])[0].id;
            let newPosts = await getPosts(props.url, upperPost, 10, false);
            gSetPostProps(newPosts.concat(posts || []));
        }
        if (
            window.scrollY ===
            document.body.scrollHeight - window.innerHeight
        ) {
            if (posts === undefined) return;
            let upperPost = posts[posts.length - 1].id;
            let newPosts = await getPosts(props.url, upperPost, 10, true);
            gSetPostProps((posts || []).concat(newPosts));
        }
    };
    return (
        <div>
            {posts &&
                posts.map(p => (
                    <Post
                        key={p.id}
                        id={p.id}
                        sender={p.sender}
                        likesCount={p.likesCount}
                        liked={p.liked}
                        time={p.time}
                        text={p.text}
                        commentCount={p.commentCount}
                    />
                ))
            // : (<Skeleton width='80%' height='200px' />)}
            }
        </div>
    );
}
