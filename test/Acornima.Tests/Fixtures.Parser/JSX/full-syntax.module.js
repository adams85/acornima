import React from 'react';
import * as Namespace from './namespace';

// 1. Basic element, self-closing
const a = <div />;

// 2. Element with children (text + expression + nested element)
const b = <div>Hello {name} <span>world</span></div>;

// 3. Multiple children, mixed text and expressions
const c = <ul><li>{1}</li><li>{2}</li><li>{3}</li></ul>;

// 4. Whitespace handling: leading/trailing spaces, newlines, blank lines
const d = (
    <div>
        Line one
        Line two

        Line three after a blank line
    </div>
);

// 5. Attributes: string, expression, boolean shorthand
const e = <input type="text" value={value} disabled />;

// 6. Spread attributes (before and after named attributes)
const f = <Comp {...props} a={1} {...moreProps} b="two" />;

// 7. Namespaced attribute (JSX namespace syntax)
const g = <svg xlink:href="#icon" xml:lang="en" />;

// 8. Namespaced element name
const h = <svg:circle r="5" />;

// 9. Member-expression element names
const i = <Namespace.Widget.Inner prop={1} />;
const j = <foo.bar.baz />;

// 10. Fragment syntax (short form)
const k = <>{a}{b}{c}</>;

// 11. Fragment with explicit React.Fragment
const l = <React.Fragment key="frag">{a}</React.Fragment>;

// 12. Element with key and ref attributes
const m = <Item key={id} ref={ref} />;

// 13. Conditional / logical expressions as children
const n = <div>{cond ? <Yes /> : <No />}{maybe && <Maybe />}</div>;

// 14. Array / map as children
const o = <ul>{items.map((it) => <li key={it.id}>{it.label}</li>)}</ul>;

// 15. Comments inside JSX children (wrapped in braces)
const p = (
    <div>
        {/* a block comment inside JSX children */}
        <span>after comment</span>
    </div>
);

// 16. Entity references in text and attribute values
const q = <div title="a &amp; b">&lt;tag&gt; &quot;quoted&quot; &#169;</div>;

// 17. String attribute containing quotes / newlines
const r = <div data-x='single "quoted"' data-y="line1
line2" />;

// 18. Expression containers with complex expressions
const s = <div>{function () { return 1; }()}{(() => 2)()}{[1, 2, 3].length}</div>;

// 19. Nested JSX inside expression containers
const t = <div>{cond ? <A>{<B />}</A> : null}</div>;

// 20. JSX inside template literals / tagged templates (just JS, but parses JSX)
const u = `${<span />}`;

// 21. JSX as a function argument / return value
function render() {
    return <Wrapper>{<Child />}</Wrapper>;
}
const v = [<A key="1" />, <B key="2" />];

// 22. JSX with reserved-word-ish attribute names and dashes
const w = <div class="x" for="y" data-foo-bar="z" aria-label="lbl" />;

// 23. Empty expression container (comment-only) — valid in JSX children
const x = <div>{/* only a comment */}</div>;

// 24. Element with only whitespace children
const y = <div>   </div>;

// 25. Deeply nested elements
const z = <A><B><C><D><E /></D></C></B></A>;

// 26. JSX in arrow function bodies (concise and block)
const aa = () => <div />;
const ab = () => { return <div />; };

// 27. Conditional rendering with nested fragments
const ac = (
    <>
        {loading ? <Spinner /> : (
            <>
                <Header />
                <Body />
            </>
        )}
    </>
);

// 28. Attributes spanning multiple lines with trailing comma-ish layouts
const ad = (
    <Comp
        a={1}
        b="two"
        {...rest}
        c={<Inner />}
    />
);

// 29. Unicode identifiers and text
const ae = <div title="日本語">こんにちは {名前}</div>;

// 30. Numeric and boolean literal attribute values (expression form)
const af = <div tabIndex={0} draggable={false} data-n={42} />;
