import { windowManager, type Window } from "node-window-manager";
import { IRectangle } from "node-window-manager/dist/interfaces";

import { type getOffsets as offsetsGetter } from "../settings";

let getOffsets: typeof offsetsGetter = () => ({
  top: 0,
  left: 0,
  bottom: 0,
  right: 0,
});
export const setOffsetsGetter = (offsetsGetter: typeof getOffsets) => {
  getOffsets = offsetsGetter;
};

const isWindowShadowed = (window: Window) => {
  const WS_EX_TOOLWINDOW = 0x00000080;
  const WS_EX_LAYERED = 0x00080000;
  const hasWS_EX_TOOLWINDOW =
    (window.id & WS_EX_TOOLWINDOW) === WS_EX_TOOLWINDOW;
  const hasWS_EX_LAYERED = (window.id & WS_EX_LAYERED) === WS_EX_LAYERED;
  const hasShadow = hasWS_EX_TOOLWINDOW || hasWS_EX_LAYERED;
  console.log({
    hasWS_EX_TOOLWINDOW,
    hasWS_EX_LAYERED,
    hasShadow,
  });
  return hasShadow;
};

const getWindowShadowOffsets = (window: Window) => {
  if (isWindowShadowed(window)) {
    return {
      x: -5,
      y: 0,
      height: 8,
      width: 30,
    };
  } else {
    return {
      x: 0,
      y: 0,
      height: 0,
      width: 0,
    };
  }
};

const fixBoundsForShadow = (bounds: Required<IRectangle>, window: Window) => {
  const shadowOffsets = getWindowShadowOffsets(window);
  return {
    x: bounds.x + shadowOffsets.x,
    y: bounds.y + shadowOffsets.y,
    width: bounds.width + shadowOffsets.width,
    height: bounds.height + shadowOffsets.height,
  };
};

const getMonitorSizes = (window: Window) => {
  const monitor = window.getMonitor();
  const { height, width, x, y } = monitor.getBounds();
  const scaleFactor = monitor.getScaleFactor();
  if (
    height === undefined ||
    width === undefined ||
    x === undefined ||
    y === undefined
  ) {
    throw new Error("Could not get real resolution");
  }
  const configuredOffsets = getOffsets!();
  return {
    height:
      (height - configuredOffsets.bottom - configuredOffsets.top) / scaleFactor,
    width:
      (width - configuredOffsets.right - configuredOffsets.left) / scaleFactor,
    x: (x + configuredOffsets.left) / scaleFactor,
    y: (y + configuredOffsets.top) / scaleFactor,
    scaleFactor,
  };
};

const isWindowAtBounds = (window: Window, bounds: IRectangle) => {
  const { height, width, x, y } = window.getBounds();
  if (
    bounds.x === undefined ||
    bounds.y === undefined ||
    bounds.width === undefined ||
    bounds.height === undefined ||
    x === undefined ||
    y === undefined ||
    width === undefined ||
    height === undefined
  ) {
    console.warn("This should not happen");
    return false;
  }

  return (
    Math.abs(bounds.x - x) < 1 &&
    Math.abs(bounds.y - y) < 1 &&
    Math.abs(bounds.width - width) < 1 &&
    Math.abs(bounds.height - height) < 1
  );
};

const fullScreen = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  return fixBoundsForShadow(
    {
      x,
      y,
      width,
      height,
    },
    window
  );
};

const leftOneThird = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  return fixBoundsForShadow(
    {
      x,
      y,
      width: width / 3,
      height,
    },
    window
  );
};

const leftTwoThirds = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  return fixBoundsForShadow(
    {
      x,
      y,
      width: (width / 3) * 2,
      height,
    },
    window
  );
};

const rightOneThird = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newWidth = width / 3;
  return fixBoundsForShadow(
    {
      x: x + newWidth * 2,
      y,
      width: width / 3,
      height,
    },
    window
  );
};

const rightTwoThirds = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newWidth = width / 3;
  return fixBoundsForShadow(
    {
      x: x + newWidth,
      y,
      width: (width / 3) * 2,
      height,
    },
    window
  );
};

const leftHalf = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  return fixBoundsForShadow(
    {
      x,
      y,
      width: width / 2,
      height,
    },
    window
  );
};

const dynamicLeftHalf = (window: Window) => {
  const leftHalfBounds = leftHalf(window);
  const leftOneThirdBounds = leftOneThird(window);
  const leftTwoThirdsBounds = leftTwoThirds(window);

  if (isWindowAtBounds(window, leftHalfBounds)) {
    return leftTwoThirds(window);
  } else if (isWindowAtBounds(window, leftTwoThirdsBounds)) {
    return leftOneThird(window);
  } else if (isWindowAtBounds(window, leftOneThirdBounds)) {
    return leftHalfBounds;
  } else {
    return leftHalfBounds;
  }
};

const dynamicRightHalf = (window: Window) => {
  const rightHalfBounds = rightHalf(window);
  const rightOneThirdBounds = rightOneThird(window);
  const rightTwoThirdsBounds = rightTwoThirds(window);

  if (isWindowAtBounds(window, rightHalfBounds)) {
    return rightTwoThirds(window);
  } else if (isWindowAtBounds(window, rightTwoThirdsBounds)) {
    return rightOneThird(window);
  } else if (isWindowAtBounds(window, rightOneThirdBounds)) {
    return rightHalfBounds;
  } else {
    return rightHalfBounds;
  }
};

const rightHalf = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  return fixBoundsForShadow(
    {
      x: x + width / 2,
      y,
      width: width / 2,
      height,
    },
    window
  );
};

const topLeftSixth = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  return fixBoundsForShadow(
    {
      x,
      y,
      width: width / 3,
      height: height / 2,
    },
    window
  );
};

const topMiddleSixth = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newWidth = width / 3;
  return fixBoundsForShadow(
    {
      x: x + newWidth,
      y,
      width: newWidth,
      height: height / 2,
    },
    window
  );
};

const topRightSixth = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newWidth = width / 3;
  return fixBoundsForShadow(
    {
      x: x + newWidth * 2,
      y,
      width: newWidth,
      height: height / 2,
    },
    window
  );
};

const bottomLeftSixth = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newHeight = height / 2;
  return fixBoundsForShadow(
    {
      x,
      y: y + newHeight,
      width: width / 3,
      height: newHeight,
    },
    window
  );
};

const bottomMiddleSixth = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newWidth = width / 3;
  const newHeight = height / 2;
  return fixBoundsForShadow(
    {
      x: x + newWidth,
      y: y + newHeight,
      width: newWidth,
      height: newHeight,
    },
    window
  );
};

const bottomRightSixth = (window: Window) => {
  const { height, width, x, y } = getMonitorSizes(window);
  const newWidth = width / 3;
  const newHeight = height / 2;
  return fixBoundsForShadow(
    {
      x: x + newWidth * 2,
      y: y + newHeight,
      width: newWidth,
      height: newHeight,
    },
    window
  );
};

export const setWindow = (window: Window, bounds: IRectangle) => {
  window.setBounds(bounds);
};

const toWindowMover = (boundsGetter: (window: Window) => IRectangle) => {
  return () => {
    const window = windowManager.getActiveWindow();
    window.setBounds(boundsGetter(window));
  };
};

export const getShortcutActions = () => {
  return {
    fullScreen: toWindowMover(fullScreen),
    leftOneThird: toWindowMover(leftOneThird),
    leftTwoThirds: toWindowMover(leftTwoThirds),
    rightOneThird: toWindowMover(rightOneThird),
    rightTwoThirds: toWindowMover(rightTwoThirds),
    leftHalf: toWindowMover(leftHalf),
    dynamicLeftHalf: toWindowMover(dynamicLeftHalf),
    dynamicRightHalf: toWindowMover(dynamicRightHalf),
    rightHalf: toWindowMover(rightHalf),
    topLeftSixth: toWindowMover(topLeftSixth),
    topMiddleSixth: toWindowMover(topMiddleSixth),
    topRightSixth: toWindowMover(topRightSixth),
    bottomLeftSixth: toWindowMover(bottomLeftSixth),
    bottomMiddleSixth: toWindowMover(bottomMiddleSixth),
    bottomRightSixth: toWindowMover(bottomRightSixth),
  } as const;
};
