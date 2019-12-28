import React from 'react';
import { useScrollTrigger, Slide } from '@material-ui/core';

interface HideOnScrollProps {
    direction: 'up' | 'down' | 'left' | 'right';
    window?: () => Window; children: React.ReactElement;
}

export default function HideOnScroll(props: HideOnScrollProps) {
    const { children, window } = props;
    // Note that you normally won't need to set the window ref as useScrollTrigger
    // will default to window.
    // This is only being set here because the demo is in an iframe.
    const trigger = useScrollTrigger({ target: window ? window() : undefined });

    return (
        <Slide appear={false} direction={props.direction} in={!trigger}>
            {children}
        </Slide>
    );
}