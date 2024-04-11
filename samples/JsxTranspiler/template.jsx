function msgSpan(data, i) {
  return <span id={`msg${i}`} {...data.attrs} {...{selected: i === 0}}><b>Message:</b> {data.msg}</span>;
}

export default function (data) {
  return <div>{[...Array(4)].map((_, i) => i > 0 ? (<><br/>{msgSpan(data, i)}</>) : msgSpan(data, i))}</div>;
}
