import React from "react";
import { BrowserRouter, Route } from "react-router-dom";
import Layout from "./Components/Layout";
import PostList from "./Components/PostList";
import SendPost from "./Pages/SendPost";
import ViewUser from "./Pages/ViewUser";
import SignIn from "./Pages/Auth/SignIn";
import SignUp from "./Pages/Auth/SignUp";
import SignOut from "./Pages/Auth/SignOut";
import ViewPost from "./Pages/ViewPost";
import Search from "./Pages/Search";
import EditUser from "./Pages/Auth/EditUser";

const baseUrl: string =
    document.getElementsByTagName("base")[0].getAttribute("href") || "";

export default function App(props: any) {
    // new RequestManager(window.location.origin.split(':')[0])
    //     .Get(new URL('API/Layout/Index'), undefined, undefined, sender => { })
    return (
        <>
            <BrowserRouter basename={baseUrl}>
                <Layout>
                    <Route exact path="/">
                        <PostList url="/API/Post/View" />
                    </Route>
                    <Route path="/SignIn">
                        <SignIn />
                    </Route>
                    <Route path="/SignUp">
                        <SignUp />
                    </Route>
                    <Route path="/SignOut">
                        <SignOut />
                    </Route>
                    <Route path="/Post/View">
                        <ViewPost />
                    </Route>
                    <Route exact path="/post/new">
                        <SendPost />
                    </Route>
                    <Route path="/user">
                        <ViewUser />
                    </Route>
                    <Route path='/EditUser'>
                        <EditUser />
                    </Route>
                    <Route path="/search">
                        <Search />
                    </Route>
                    <Route path='/'>
                        <></>
                    </Route>
                </Layout>
            </BrowserRouter>
        </>
        // <BrowserRouter basename={baseUrl}>
        //     <Route exact path="/">Hi</Route>
        //     <Route path="/about">About</Route>
        // </BrowserRouter>
    );
}
