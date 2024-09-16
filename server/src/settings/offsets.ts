let offsets = {
  top: 0,
  left: 0,
  bottom: 0,
  right: 0,
};

export const getOffsets = () => offsets;
export const setOffsets = (newOffsets: typeof offsets) => {
  const top = newOffsets.top;
  const left = newOffsets.left;
  const bottom = Math.abs(top) + newOffsets.bottom;
  const right = Math.abs(left) + newOffsets.right;
  offsets = { top, left, bottom, right };
};
