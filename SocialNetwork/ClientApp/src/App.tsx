import { BrowserRouter, Route, Routes } from "react-router";
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
import authService from "./Services/AuthorizeService";

export default function App() {
  let isSigned = authService.isSigned();
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route
            index
            element={
              isSigned ? (
                <PostList url="/API/Post/View" />
              ) : (
                <PostList url="/API/Post/Trends" />
              )
            }
          ></Route>
          <Route path="/SignIn" element={<SignIn />} />
          <Route path="/SignUp" element={<SignUp />} />
          <Route path="/SignOut" element={<SignOut />} />
          <Route path="/Post/View/:id" element={<ViewPost />} />
          <Route index path="/post/new" element={<SendPost />} />
          <Route path="/user/:id" element={<ViewUser />} />
          <Route path="/EditUser" element={<EditUser />} />
          <Route path="/search" element={<Search />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
