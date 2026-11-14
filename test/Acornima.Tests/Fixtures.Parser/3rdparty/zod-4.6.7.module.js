// \0rolldown/runtime.js
var e = Object.defineProperty, t = (t, n) => {
	let r = {};
	for (var i in t) e(r, i, {
		get: t[i],
		enumerable: !0
	});
	return n || e(r, Symbol.toStringTag, { value: "Module" }), r;
}, n = /* @__PURE__ */ t({
	BIGINT_FORMAT_RANGES: () => De,
	CONSTANT_CATCH: () => ut,
	Class: () => $e,
	NUMBER_FORMAT_RANGES: () => Ee,
	aborted: () => C,
	allowsEval: () => ge,
	assert: () => c,
	assertEqual: () => r,
	assertIs: () => o,
	assertNever: () => s,
	assertNotEqual: () => i,
	assignProp: () => _,
	attachSchema: () => Be,
	base64ToUint8Array: () => qe,
	base64urlToUint8Array: () => Ye,
	cached: () => p,
	captureStackTrace: () => me,
	cleanEnum: () => Ke,
	cleanRegex: () => h,
	clone: () => b,
	cloneDef: () => le,
	codePointLength: () => Ue,
	constantCatch: () => dt,
	createTransparentProxy: () => we,
	defineLazy: () => te,
	defineLazyInternal: () => D,
	derived: () => rt,
	esc: () => y,
	escapeRegex: () => Ce,
	explicitlyAborted: () => Le,
	extend: () => je,
	finalizeIssue: () => T,
	floatSafeRemainder: () => g,
	getElementAtPath: () => ue,
	getEnumValues: () => l,
	getLengthableOrigin: () => We,
	getParsedType: () => be,
	getSizableOrigin: () => Ve,
	hexToUint8Array: () => Ze,
	hide: () => nt,
	installLazyProp: () => lt,
	isObject: () => he,
	isPlainObject: () => _e,
	issue: () => Ge,
	joinValues: () => u,
	jsonStringifyReplacer: () => d,
	members: () => et,
	merge: () => Pe,
	mergeDefs: () => v,
	normalizeParams: () => x,
	nullish: () => m,
	numKeys: () => ye,
	objectClone: () => ne,
	omit: () => Ae,
	optionalKeys: () => Te,
	own: () => tt,
	parsedType: () => E,
	partial: () => Fe,
	pick: () => Oe,
	prefixIssues: () => w,
	primitiveTypes: () => Se,
	promiseAllObject: () => de,
	propertyKeyTypes: () => xe,
	randomString: () => fe,
	rawShape: () => re,
	required: () => Ie,
	safeExtend: () => Ne,
	shallowClone: () => ve,
	sizeCaps: () => ze,
	slugify: () => pe,
	stringifyPrimitive: () => S,
	toZod: () => a,
	uint8ArrayToBase64: () => Je,
	uint8ArrayToBase64url: () => Xe,
	uint8ArrayToHex: () => Qe,
	unwrapMessage: () => Re
});
function r(e) {
	return e;
}
function i(e) {
	return e;
}
function a() {
	return (e) => e;
}
function o(e) {}
function s(e) {
	throw Error("Unexpected value in exhaustive check");
}
function c(e) {}
function l(e) {
	let t = Object.values(e).filter((e) => typeof e == "number");
	return Object.entries(e).filter(([e, n]) => t.indexOf(+e) === -1).map(([e, t]) => t);
}
function u(e, t = "|") {
	return e.map((e) => S(e)).join(t);
}
function d(e, t) {
	return typeof t == "bigint" ? t.toString() : t;
}
var f = class {
	constructor(e) {
		this._getter = e, this._value = void 0;
	}
	get value() {
		let e = this._getter;
		return e !== void 0 && (this._value = e(), this._getter = void 0), this._value;
	}
};
function p(e) {
	return new f(e);
}
function m(e) {
	return e == null;
}
function h(e) {
	let t = +!!e.startsWith("^"), n = e.endsWith("$") ? e.length - 1 : e.length;
	return e.slice(t, n);
}
function g(e, t) {
	let n = e / t, r = Math.round(n), i = 4 * 2 ** -52 * Math.max(Math.abs(n), 1);
	return Math.abs(n - r) < i ? 0 : n - r;
}
var ee = /* @__PURE__*/ Symbol("evaluating");
function te(e, t, n) {
	let r;
	Object.defineProperty(e, t, {
		get() {
			if (r !== ee) return r === void 0 && (r = ee, r = n()), r;
		},
		set(n) {
			Object.defineProperty(e, t, { value: n });
		},
		configurable: !0
	});
}
function ne(e) {
	return Object.create(Object.getPrototypeOf(e), Object.getOwnPropertyDescriptors(e));
}
function _(e, t, n) {
	Object.defineProperty(e, t, {
		value: n,
		writable: !0,
		enumerable: !0,
		configurable: !0
	});
}
function re(e) {
	let t = Object.getOwnPropertyDescriptor(e, "shape");
	return t?.get ? t.get.raw : t?.value;
}
function ie(e) {
	return re(e._zod.def) ?? e._zod.def.shape;
}
function ae(e, t, n) {
	Object.defineProperty(e, t, {
		get() {
			let e = n();
			return _(this, t, e), e;
		},
		enumerable: !0,
		configurable: !0
	});
}
function oe(e, t, n) {
	t in e ? _(e, t, n) : e[t] = n;
}
function se(e, t, n, r) {
	let i = ie(t);
	for (let a of n) {
		let n = Object.getOwnPropertyDescriptor(i, a);
		n.enumerable && (n.get ? ae(e, a, () => {
			let e = t._zod.def.shape[a];
			return r ? r(e, a) : e;
		}) : oe(e, a, r ? r(n.value, a) : n.value));
	}
}
function ce(e, t) {
	for (let n of Reflect.ownKeys(t)) {
		let r = Object.getOwnPropertyDescriptor(t, n);
		r.enumerable && (r.get ? ae(e, n, () => t[n]) : oe(e, n, r.value));
	}
}
function v(...e) {
	let t = {};
	for (let n of e) {
		let e = Object.getOwnPropertyDescriptors(n);
		Object.assign(t, e);
	}
	return Object.defineProperties({}, t);
}
function le(e) {
	return v(e._zod.def);
}
function ue(e, t) {
	return t ? t.reduce((e, t) => e?.[t], e) : e;
}
function de(e) {
	let t = Object.keys(e), n = t.map((t) => e[t]);
	return Promise.all(n).then((e) => {
		let n = {};
		for (let r = 0; r < t.length; r++) n[t[r]] = e[r];
		return n;
	});
}
function fe(e = 10) {
	let t = "";
	for (let n = 0; n < e; n++) t += "abcdefghijklmnopqrstuvwxyz"[Math.floor(Math.random() * 26)];
	return t;
}
function y(e) {
	return JSON.stringify(e);
}
function pe(e) {
	return e.toLowerCase().trim().replace(/[^\w\s-]/g, "").replace(/[\s_-]+/g, "-").replace(/^-+|-+$/g, "");
}
var me = "captureStackTrace" in Error ? Error.captureStackTrace : (...e) => {};
function he(e) {
	return typeof e == "object" && !!e && !Array.isArray(e);
}
var ge = /* @__PURE__*/ p(() => {
	if (k.jitless || typeof navigator < "u" && navigator?.userAgent?.includes("Cloudflare")) return !1;
	try {
		return Function(""), !0;
	} catch {
		return !1;
	}
});
function _e(e) {
	if (he(e) === !1) return !1;
	let t = e.constructor;
	if (t === void 0 || typeof t != "function") return !0;
	let n = t.prototype;
	return he(n) !== !1 && Object.prototype.hasOwnProperty.call(n, "isPrototypeOf") !== !1;
}
function ve(e) {
	return _e(e) ? { ...e } : Array.isArray(e) ? [...e] : e instanceof Map ? new Map(e) : e instanceof Set ? new Set(e) : e;
}
function ye(e) {
	let t = 0;
	for (let n in e) Object.prototype.hasOwnProperty.call(e, n) && t++;
	return t;
}
var be = (e) => {
	let t = typeof e;
	switch (t) {
		case "undefined": return "undefined";
		case "string": return "string";
		case "number": return Number.isNaN(e) ? "nan" : "number";
		case "boolean": return "boolean";
		case "function": return "function";
		case "bigint": return "bigint";
		case "symbol": return "symbol";
		case "object": return Array.isArray(e) ? "array" : e === null ? "null" : e.then && typeof e.then == "function" && e.catch && typeof e.catch == "function" ? "promise" : typeof Map < "u" && e instanceof Map ? "map" : typeof Set < "u" && e instanceof Set ? "set" : typeof Date < "u" && e instanceof Date ? "date" : typeof File < "u" && e instanceof File ? "file" : "object";
		default: throw Error(`Unknown data type: ${t}`);
	}
}, xe = /* @__PURE__*/ new Set([
	"string",
	"number",
	"symbol"
]), Se = /* @__PURE__*/ new Set([
	"string",
	"number",
	"bigint",
	"boolean",
	"symbol",
	"undefined"
]);
function Ce(e) {
	return e.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}
function b(e, t, n) {
	let r = new e._zod.constr(t ?? e._zod.def);
	return (!t || n?.parent) && (r._zod.parent = e), r;
}
function x(e) {
	let t = e;
	if (!t) return {};
	if (typeof t == "string") return { error: () => t };
	if (t?.message !== void 0) {
		if (t?.error !== void 0) throw Error("Cannot specify both `message` and `error` params");
		t.error = t.message;
	}
	return delete t.message, typeof t.error == "string" ? {
		...t,
		error: () => t.error
	} : t;
}
function we(e) {
	let t;
	return new Proxy({}, {
		get(n, r, i) {
			return t ??= e(), Reflect.get(t, r, i);
		},
		set(n, r, i, a) {
			return t ??= e(), Reflect.set(t, r, i, a);
		},
		has(n, r) {
			return t ??= e(), Reflect.has(t, r);
		},
		deleteProperty(n, r) {
			return t ??= e(), Reflect.deleteProperty(t, r);
		},
		ownKeys(n) {
			return t ??= e(), Reflect.ownKeys(t);
		},
		getOwnPropertyDescriptor(n, r) {
			return t ??= e(), Reflect.getOwnPropertyDescriptor(t, r);
		},
		defineProperty(n, r, i) {
			return t ??= e(), Reflect.defineProperty(t, r, i);
		}
	});
}
function S(e) {
	return typeof e == "bigint" ? e.toString() + "n" : typeof e == "string" ? `"${e}"` : `${e}`;
}
function Te(e) {
	return Object.keys(e).filter((t) => e[t]._zod.optin !== void 0 && e[t]._zod.optout === "optional");
}
var Ee = {
	safeint: [-(2 ** 53 - 1), 2 ** 53 - 1],
	int32: [-2147483648, 2147483647],
	uint32: [0, 4294967295],
	float32: [-34028234663852886e22, 34028234663852886e22],
	float64: [-Number.MAX_VALUE, Number.MAX_VALUE]
}, De = {
	int64: [/* @__PURE__*/ BigInt("-9223372036854775808"), /* @__PURE__*/ BigInt("9223372036854775807")],
	uint64: [/* @__PURE__*/ BigInt(0), /* @__PURE__*/ BigInt("18446744073709551615")]
};
function Oe(e, t) {
	let n = e._zod.def, r = n.checks;
	if (r && r.length > 0) throw Error(".pick() cannot be used on object schemas containing refinements");
	let i = {};
	return se(i, e, ke(e, t)), b(e, v(n, {
		shape: i,
		checks: []
	}));
}
function ke(e, t) {
	let n = ie(e), r = [];
	for (let e of Reflect.ownKeys(t)) {
		if (!Object.getOwnPropertyDescriptor(n, e)?.enumerable) throw Error(`Unrecognized key: "${String(e)}"`);
		t[e] && r.push(e);
	}
	return r;
}
function Ae(e, t) {
	let n = e._zod.def, r = n.checks;
	if (r && r.length > 0) throw Error(".omit() cannot be used on object schemas containing refinements");
	let i = new Set(ke(e, t)), a = {};
	return se(a, e, Reflect.ownKeys(ie(e)).filter((e) => !i.has(e))), b(e, v(n, {
		shape: a,
		checks: []
	}));
}
function je(e, t) {
	if (!_e(t)) throw Error("Invalid input to extend: expected a plain object");
	let n = e._zod.def.checks;
	if (n && n.length > 0) {
		let n = ie(e);
		for (let e of Reflect.ownKeys(t)) if (Object.getOwnPropertyDescriptor(n, e) !== void 0) throw Error("Cannot overwrite keys on object schemas containing refinements. Use `.safeExtend()` instead.");
	}
	return b(e, v(e._zod.def, { shape: Me(e, t) }));
}
function Me(e, t) {
	let n = {};
	return se(n, e, Reflect.ownKeys(ie(e))), ce(n, t), n;
}
function Ne(e, t) {
	if (!_e(t)) throw Error("Invalid input to safeExtend: expected a plain object");
	return b(e, v(e._zod.def, { shape: Me(e, t) }));
}
function Pe(e, t) {
	if (!t?._zod?.def) throw Error("Invalid input to merge: expected an object schema. To merge a plain shape, use `.extend()`.");
	if (e._zod.def.checks?.length) throw Error(".merge() cannot be used on object schemas containing refinements. Use .safeExtend() instead.");
	let n = {};
	return se(n, e, Reflect.ownKeys(ie(e))), se(n, t, Reflect.ownKeys(ie(t))), b(e, v(e._zod.def, {
		shape: n,
		get catchall() {
			return t._zod.def.catchall;
		},
		checks: t._zod.def.checks ?? []
	}));
}
function Fe(e, t, n, r = "partial") {
	let i = t._zod.def.checks;
	if (i && i.length > 0) throw Error(`.${r}() cannot be used on object schemas containing refinements`);
	let a = n ? new Set(ke(t, n)) : void 0, o = {};
	return se(o, t, Reflect.ownKeys(ie(t)), e && ((t, n) => a && !a.has(n) ? t : new e({
		type: "optional",
		innerType: t
	}))), b(t, v(t._zod.def, {
		shape: o,
		checks: []
	}));
}
function Ie(e, t, n) {
	let r = n ? new Set(ke(t, n)) : void 0, i = {};
	return se(i, t, Reflect.ownKeys(ie(t)), (t, n) => r && !r.has(n) ? t : new e({
		type: "nonoptional",
		innerType: t
	})), b(t, v(t._zod.def, { shape: i }));
}
function C(e, t = 0) {
	if (e.aborted === !0) return !0;
	for (let n = t; n < e.issues.length; n++) if (e.issues[n]?.continue !== !0) return !0;
	return !1;
}
function Le(e, t = 0) {
	if (e.aborted === !0) return !0;
	for (let n = t; n < e.issues.length; n++) if (e.issues[n]?.continue === !1) return !0;
	return !1;
}
function w(e, t) {
	return t.map((t) => {
		var n;
		return (n = t).path ?? (n.path = []), t.path.unshift(e), t;
	});
}
function Re(e) {
	return typeof e == "string" ? e : e?.message;
}
function ze(e) {
	let t = [];
	for (let n of e ?? []) {
		let e = n._zod.def;
		if (!/_(length|size)$|_equals$/.test(e.check)) break;
		let r = e.maximum ?? e.length ?? e.size;
		r >= 0 && t.push([r, n]);
	}
	return t;
}
function Be(e, t, n) {
	var r;
	for (let i = t; i < e.length; i++) (r = e[i]).schema ?? (r.schema = n);
}
function T(e, t, n) {
	var r;
	let i = e.inst?._zod?.traits;
	i?.has("$ZodType") && (i.has("$ZodCheck") ? (r = e).schema ?? (r.schema = e.inst) : e.schema = e.inst);
	let a = e.schema === e.inst ? void 0 : e.schema?._zod.def?.error, o = e.message ? e.message : Re(e.inst?._zod.def?.error?.(e)) ?? Re(a?.(e)) ?? Re(t?.error?.(e)) ?? Re(n.customError?.(e)) ?? Re(n.localeError?.(e)) ?? "Invalid input", s = {};
	for (let t of Object.keys(e)) t !== "inst" && t !== "schema" && t !== "continue" && t !== "input" && t !== "__proto__" && (s[t] = e[t]);
	return s.path ??= [], s.message = o, t?.reportInput && (s.input = e.input), s;
}
function Ve(e) {
	return e instanceof Set ? "set" : e instanceof Map ? "map" : e instanceof File ? "file" : "unknown";
}
var He = /[\uD800-\uDBFF]/;
function Ue(e) {
	let t = e.length;
	if (!He.test(e)) return t;
	let n = t;
	for (let r = 0; r < t - 1; r++) (e.charCodeAt(r) & 64512) == 55296 && (e.charCodeAt(r + 1) & 64512) == 56320 && (n--, r++);
	return n;
}
function We(e) {
	return Array.isArray(e) ? "array" : typeof e == "string" ? "string" : "unknown";
}
function E(e) {
	let t = typeof e;
	switch (t) {
		case "number": return Number.isNaN(e) ? "nan" : "number";
		case "object": {
			if (e === null) return "null";
			if (Array.isArray(e)) return "array";
			let t = e;
			if (t && Object.getPrototypeOf(t) !== Object.prototype && "constructor" in t && t.constructor) return t.constructor.name;
		}
	}
	return t;
}
function Ge(...e) {
	let [t, n, r] = e;
	return typeof t == "string" ? {
		message: t,
		code: "custom",
		input: n,
		inst: r
	} : { ...t };
}
function Ke(e) {
	return Object.entries(e).filter(([e, t]) => Number.isNaN(Number.parseInt(e, 10))).map((e) => e[1]);
}
function qe(e) {
	let t = atob(e), n = new Uint8Array(t.length);
	for (let e = 0; e < t.length; e++) n[e] = t.charCodeAt(e);
	return n;
}
function Je(e) {
	let t = "";
	for (let n = 0; n < e.length; n++) t += String.fromCharCode(e[n]);
	return btoa(t);
}
function Ye(e) {
	let t = e.replace(/-/g, "+").replace(/_/g, "/");
	return qe(t + "=".repeat((4 - t.length % 4) % 4));
}
function Xe(e) {
	return Je(e).replace(/\+/g, "-").replace(/\//g, "_").replace(/=/g, "");
}
function Ze(e) {
	let t = e.replace(/^0x/, "");
	if (t.length % 2 != 0) throw Error("Invalid hex string length");
	let n = new Uint8Array(t.length / 2);
	for (let e = 0; e < t.length; e += 2) n[e / 2] = Number.parseInt(t.slice(e, e + 2), 16);
	return n;
}
function Qe(e) {
	return Array.from(e).map((e) => e.toString(16).padStart(2, "0")).join("");
}
var $e = class {
	constructor(...e) {}
};
function et(e, t) {
	for (let n in t) {
		let r = Object.getOwnPropertyDescriptor(t, n);
		r.get ? Object.defineProperty(e, n, {
			...r,
			enumerable: !1
		}) : it(e, n, r.value);
	}
}
function tt(e, t, n, r = !0) {
	return Object.defineProperty(e, t, {
		configurable: !0,
		writable: !0,
		enumerable: r,
		value: n
	}), n;
}
function nt(e, t, n) {
	return tt(e, t, n, !1);
}
/* @__NO_SIDE_EFFECTS__ */
function rt(e, t) {
	for (let n in e) {
		let r = e[n];
		Object.defineProperty(t, n, {
			configurable: !0,
			enumerable: !0,
			get() {
				return tt(this, n, r(this));
			},
			set(e) {
				tt(this, n, e);
			}
		});
	}
	return t;
}
function it(e, t, n) {
	Object.defineProperty(e, t, {
		configurable: !0,
		get() {
			return this == null ? n : tt(this, t, n.bind(this));
		},
		set(e) {
			tt(this, t, e);
		}
	});
}
function at(e, t) {
	let n = Object.getPrototypeOf(e);
	return t in n ? void 0 : n;
}
var ot, st = !1, ct = {
	configurable: !0,
	get() {
		st = !0;
	}
};
function D(e, t, n) {
	let r = Object.getPrototypeOf(e._zod);
	t in r && ot !== e._zod ? ot = void 0 : (ot = e._zod, Object.defineProperty(r, t, {
		configurable: !0,
		get() {
			Object.defineProperty(this, t, ct);
			let e = st;
			st = !1;
			try {
				let r = n(this);
				return st ? delete this[t] : Object.defineProperty(this, t, {
					configurable: !0,
					writable: !0,
					value: r
				}), st ||= e, r;
			} catch (n) {
				throw delete this[t], st ||= e, n;
			}
		},
		set(e) {
			Object.defineProperty(this, t, {
				configurable: !0,
				writable: !0,
				value: e
			});
		}
	}));
}
function lt(e, t, n, r) {
	let i = at(e, t);
	i && Object.defineProperty(i, t, {
		configurable: !0,
		get() {
			let e = {
				configurable: !0,
				writable: !0,
				enumerable: r,
				value: void 0
			};
			return Object.defineProperty(this, t, e), e.value = n(this), Object.defineProperty(this, t, e), e.value;
		},
		set(e) {
			Object.defineProperty(this, t, {
				configurable: !0,
				writable: !0,
				enumerable: r,
				value: e
			});
		}
	});
}
var ut = "~constantCatch";
function dt(e) {
	let t = () => e;
	return t[ut] = !0, t;
}
// node_modules/zod/v4/core/core.js
var ft, pt = /*@__PURE__*/ Object.freeze({ status: "aborted" }), mt = {
	value: void 0,
	enumerable: !1
}, ht = "captureStackTrace" in Error ? Error : null;
function gt(e) {
	let t = ht;
	if (t) {
		let n = t.stackTraceLimit;
		if (typeof n == "number") {
			try {
				t.stackTraceLimit = 0;
			} catch {
				return ht = null, new e();
			}
			try {
				return new e();
			} finally {
				t.stackTraceLimit = n;
			}
		}
	}
	return new e();
}
/* @__NO_SIDE_EFFECTS__ */
function O(e, t, n, r) {
	let i = {};
	function a(e) {
		this.def = e, this.constr = d, this.traits = /* @__PURE__ */ new Set();
	}
	a.prototype = i;
	let o = n, s = o && /* @__PURE__ */ new WeakSet();
	function c(n, r) {
		if (!n._zod) {
			mt.value = new a(r);
			try {
				Object.defineProperty(n, "_zod", mt);
			} finally {
				mt.value = void 0;
			}
		} else if (n._zod.traits.has(e)) return;
		if (n._zod.traits.add(e), t(n, r), s) {
			let e = Object.getPrototypeOf(n), t = n._zod.constr.prototype, r = e;
			for (; r && r !== t;) r = Object.getPrototypeOf(r);
			let i = r ?? e;
			s.has(i) || (s.add(i), et(i, o));
		}
		let i = d.prototype;
		for (let e in i) Object.prototype.hasOwnProperty.call(i, e) && (e in n || (n[e] = i[e].bind(n)));
	}
	let l = r?.Parent ?? Object;
	class u extends l {}
	Object.defineProperty(u, "name", { value: e });
	function d(e) {
		let t = r?.Parent ? gt(u) : this;
		c(t, e);
		let n = t._zod.deferred;
		if (n) {
			for (let e of n) e();
			t._zod.deferred = void 0;
		}
		let i = globalThis.__zod_globalConfig?.postProcessor;
		return i && i(t), t;
	}
	return Object.defineProperty(d, "init", { value: c }), Object.defineProperty(d, Symbol.hasInstance, { value: (t) => r?.Parent && t instanceof r.Parent ? !0 : t?._zod?.traits?.has(e) }), Object.defineProperty(d, "name", { value: e }), d;
}
var _t = /*@__PURE__*/ Symbol("zod_brand"), vt = class extends Error {
	constructor() {
		super("Encountered Promise during synchronous parse. Use .parseAsync() instead.");
	}
}, yt = class extends Error {
	constructor(e) {
		super(`Encountered unidirectional transform during encode: ${e}`), this.name = "ZodEncodeError";
	}
};
(ft = globalThis).__zod_globalConfig ?? (ft.__zod_globalConfig = {});
var k = globalThis.__zod_globalConfig;
function A(e) {
	return e && Object.assign(k, e), k;
}
// node_modules/zod/v4/core/errors.js
function bt() {
	let e = this._zod;
	return e.message ??= JSON.stringify(e.def, d, 2), e.message;
}
function xt(e) {
	this._zod.message = e;
}
var St = {
	get: bt,
	set: xt,
	enumerable: !0,
	configurable: !0
}, Ct = {
	value: void 0,
	enumerable: !1
}, wt = /* @__PURE__ */ new WeakSet([Object.prototype, Error.prototype]), Tt = (e, t) => {
	e.name = "$ZodError", Ct.value = t, Object.defineProperty(e, "issues", Ct), Ct.value = void 0, Object.defineProperty(e, "message", St);
	let n = Object.getPrototypeOf(e);
	wt.has(n) || (wt.add(n), Object.defineProperty(n, "toString", {
		configurable: !0,
		enumerable: !1,
		get() {
			let e = () => this.message;
			return Object.defineProperty(this, "toString", {
				value: e,
				configurable: !0,
				writable: !0
			}), e;
		},
		set(e) {
			Object.defineProperty(this, "toString", {
				value: e,
				configurable: !0,
				writable: !0
			});
		}
	}));
}, Et = /* @__PURE__ */ O("$ZodError", Tt), j = /* @__PURE__ */ O("$ZodError", Tt, void 0, { Parent: Error });
function Dt(e, t, n) {
	return Object.prototype.hasOwnProperty.call(e, t) || (t === "__proto__" ? Object.defineProperty(e, t, {
		value: n(),
		writable: !0,
		enumerable: !0,
		configurable: !0
	}) : e[t] = n()), e[t];
}
function Ot(e, t = (e) => e.message) {
	let n = {}, r = [];
	for (let i of e.issues) i.path.length > 0 ? Dt(n, i.path[0], () => []).push(t(i)) : r.push(t(i));
	return {
		formErrors: r,
		fieldErrors: n
	};
}
function kt(e, t = (e) => e.message) {
	let n = { _errors: [] }, r = (e, i = []) => {
		for (let a of e.issues) if (a.code === "invalid_union" && a.errors.length) a.errors.map((e) => r({ issues: e }, [...i, ...a.path]));
		else if (a.code === "invalid_key") r({ issues: a.issues }, [...i, ...a.path]);
		else if (a.code === "invalid_element") r({ issues: a.issues }, [...i, ...a.path]);
		else {
			let e = [...i, ...a.path];
			if (e.length === 0) n._errors.push(t(a));
			else {
				let r = n, i = 0;
				for (; i < e.length;) {
					let n = e[i], o = i === e.length - 1;
					if (n === "_errors") {
						o && r._errors.push(t(a)), i++;
						continue;
					}
					Object.prototype.hasOwnProperty.call(r, n) || Object.defineProperty(r, n, {
						value: { _errors: [] },
						enumerable: !0,
						writable: !0,
						configurable: !0
					});
					let s = r[n];
					o && s._errors.push(t(a)), r = s, i++;
				}
			}
		}
	};
	return r(e), n;
}
function At(e, t = (e) => e.message) {
	let n = { errors: [] }, r = (e, i = []) => {
		var a;
		for (let o of e.issues) if (o.code === "invalid_union" && o.errors.length) o.errors.map((e) => r({ issues: e }, [...i, ...o.path]));
		else if (o.code === "invalid_key") r({ issues: o.issues }, [...i, ...o.path]);
		else if (o.code === "invalid_element") r({ issues: o.issues }, [...i, ...o.path]);
		else {
			let e = [...i, ...o.path];
			if (e.length === 0) {
				n.errors.push(t(o));
				continue;
			}
			let r = n, s = 0;
			for (; s < e.length;) {
				let n = e[s], i = s === e.length - 1;
				typeof n == "string" ? (r.properties ?? (r.properties = {}), Object.prototype.hasOwnProperty.call(r.properties, n) || Object.defineProperty(r.properties, n, {
					value: { errors: [] },
					enumerable: !0,
					writable: !0,
					configurable: !0
				}), r = r.properties[n]) : (r.items ?? (r.items = []), (a = r.items)[n] ?? (a[n] = { errors: [] }), r = r.items[n]), i && r.errors.push(t(o)), s++;
			}
		}
	};
	return r(e), n;
}
function jt(e) {
	let t = [], n = e.map((e) => typeof e == "object" ? e.key : e);
	for (let e of n) typeof e == "number" ? t.push(`[${e}]`) : typeof e == "symbol" ? t.push(`[${JSON.stringify(String(e))}]`) : /[^\w$]/.test(e) ? t.push(`[${JSON.stringify(e)}]`) : (t.length && t.push("."), t.push(e));
	return t.join("");
}
function Mt(e) {
	let t = [], n = [...e.issues].sort((e, t) => (e.path ?? []).length - (t.path ?? []).length);
	for (let e of n) t.push(`✖ ${e.message}`), e.path?.length && t.push(`  → at ${jt(e.path)}`);
	return t.join("\n");
}
// node_modules/zod/v4/core/parse.js
function Nt(e, t) {
	return {
		callee: t?.callee ?? e,
		Err: t?.Err
	};
}
var Pt = (e) => {
	let t = (n, r, i, a) => {
		let o = i ? {
			...i,
			async: !1
		} : { async: !1 }, s = n._zod.run({
			value: r,
			issues: []
		}, o);
		if (s instanceof Promise) throw new vt();
		if (s.issues.length) {
			let n = new ((a?.Err) ?? e)(s.issues.map((e) => T(e, o, A())));
			throw me(n, a?.callee ?? t), n;
		}
		return s.value;
	};
	return t;
}, Ft = /* @__PURE__*/ Pt(j), It = (e) => {
	let t = async (n, r, i, a) => {
		let o = i ? {
			...i,
			async: !0
		} : { async: !0 }, s = n._zod.run({
			value: r,
			issues: []
		}, o);
		if (s instanceof Promise && (s = await s), s.issues.length) {
			let n = new ((a?.Err) ?? e)(s.issues.map((e) => T(e, o, A())));
			throw me(n, a?.callee ?? t), n;
		}
		return s.value;
	};
	return t;
}, Lt = /* @__PURE__*/ It(j), Rt = (e) => (t, n, r) => {
	let i = r ? {
		...r,
		async: !1
	} : { async: !1 }, a = t._zod.run({
		value: n,
		issues: []
	}, i);
	if (a instanceof Promise) throw new vt();
	return a.issues.length ? Bt(e, a.issues, i) : {
		success: !0,
		data: a.value
	};
}, zt = /* @__PURE__*/ Rt(j);
function Bt(e, t, n) {
	let r;
	return {
		success: !1,
		get error() {
			return r || (r = new e(t.map((e) => T(e, n, A()))), t = void 0, n = void 0), r;
		},
		set error(e) {
			r = e, t = void 0, n = void 0;
		}
	};
}
var Vt = (e) => async (t, n, r) => {
	let i = r ? {
		...r,
		async: !0
	} : { async: !0 }, a = t._zod.run({
		value: n,
		issues: []
	}, i);
	return a instanceof Promise && (a = await a), a.issues.length ? Bt(e, a.issues, i) : {
		success: !0,
		data: a.value
	};
}, Ht = /* @__PURE__*/ Vt(j), Ut = /* @__PURE__ */ Symbol.for("zod.compile.invalid"), Wt = /* @__PURE__ */ Symbol.for("zod.compile.fallback"), Gt = ((e, t, n) => {
	let r = e._zod.bag.validator;
	if (r !== void 0) {
		if (r(t) !== Ut) return !0;
		if (r.definite === !0 && n === void 0) return !1;
	}
	return Kt(e, t, n);
});
function Kt(e, t, n) {
	let r = n ? {
		...n,
		async: !1,
		abortEarly: !0
	} : {
		async: !1,
		abortEarly: !0
	}, i = e._zod.bag.fallbackRun, a;
	if (i ? (r[Wt] = !0, a = i({
		value: t,
		issues: []
	}, r)) : a = e._zod.run({
		value: t,
		issues: []
	}, r), a instanceof Promise) throw new vt();
	return a.issues.length === 0;
}
var qt = async (e, t, n) => {
	let r = n ? {
		...n,
		async: !0,
		abortEarly: !0
	} : {
		async: !0,
		abortEarly: !0
	}, i = e._zod.run({
		value: t,
		issues: []
	}, r);
	return i instanceof Promise && (i = await i), i.issues.length === 0;
}, Jt = (e) => {
	let t = Pt(e), n = (e, r, i, a) => {
		let o = i ? {
			...i,
			direction: "backward"
		} : { direction: "backward" };
		return t(e, r, o, Nt(n, a));
	};
	return n;
}, Yt = /* @__PURE__*/ Jt(j), Xt = (e) => {
	let t = Pt(e), n = (e, r, i, a) => t(e, r, i, Nt(n, a));
	return n;
}, Zt = /* @__PURE__*/ Xt(j), Qt = (e) => {
	let t = It(e), n = async (e, r, i, a) => {
		let o = i ? {
			...i,
			direction: "backward"
		} : { direction: "backward" };
		return await t(e, r, o, Nt(n, a));
	};
	return n;
}, $t = /* @__PURE__*/ Qt(j), en = (e) => {
	let t = It(e), n = async (e, r, i, a) => await t(e, r, i, Nt(n, a));
	return n;
}, tn = /* @__PURE__*/ en(j), nn = (e) => (t, n, r) => {
	let i = r ? {
		...r,
		direction: "backward"
	} : { direction: "backward" };
	return Rt(e)(t, n, i);
}, rn = /* @__PURE__*/ nn(j), an = (e) => (t, n, r) => Rt(e)(t, n, r), on = /* @__PURE__*/ an(j), sn = (e) => async (t, n, r) => {
	let i = r ? {
		...r,
		direction: "backward"
	} : { direction: "backward" };
	return Vt(e)(t, n, i);
}, cn = /* @__PURE__*/ sn(j), ln = (e) => async (t, n, r) => Vt(e)(t, n, r), un = /* @__PURE__*/ ln(j), dn = /* @__PURE__ */ t({
	anyString: () => er,
	base64: () => zn,
	base64url: () => Bn,
	bigint: () => nr,
	boolean: () => ar,
	browserEmail: () => jn,
	cidrv4: () => Ln,
	cidrv6: () => Rn,
	creditCard: () => Gn,
	cuid: () => fn,
	cuid2: () => pn,
	currencyCode: () => Kn,
	date: () => Xn,
	datetime: () => $n,
	domain: () => Hn,
	duration: () => yn,
	e164: () => Wn,
	email: () => En,
	emoji: () => Nn,
	extendedDuration: () => bn,
	guid: () => xn,
	hex: () => ur,
	hostname: () => Vn,
	html5Email: () => Dn,
	httpProtocol: () => Un,
	iban: () => qn,
	idnEmail: () => An,
	integer: () => rr,
	ipv4: () => Pn,
	ipv6: () => Fn,
	ksuid: () => gn,
	lowercase: () => cr,
	mac: () => In,
	md5_base64: () => mr,
	md5_base64url: () => hr,
	md5_hex: () => pr,
	nanoid: () => _n,
	nanoidOfLength: () => vn,
	null: () => or,
	number: () => ir,
	rfc5322Email: () => On,
	sha1_base64: () => _r,
	sha1_base64url: () => vr,
	sha1_hex: () => gr,
	sha256_base64: () => br,
	sha256_base64url: () => xr,
	sha256_hex: () => yr,
	sha384_base64: () => Cr,
	sha384_base64url: () => wr,
	sha384_hex: () => Sr,
	sha512_base64: () => Er,
	sha512_base64url: () => Dr,
	sha512_hex: () => Tr,
	string: () => tr,
	time: () => Qn,
	ulid: () => mn,
	undefined: () => sr,
	unicodeEmail: () => kn,
	uppercase: () => lr,
	uuid: () => Sn,
	uuid4: () => Cn,
	uuid6: () => wn,
	uuid7: () => Tn,
	xid: () => hn
}), fn = /^[cC][0-9a-z]{6,}$/, pn = /^[0-9a-z]+$/, mn = /^[0-7][0-9A-HJKMNP-TV-Za-hjkmnp-tv-z]{25}$/, hn = /^[0-9a-vA-V]{20}$/, gn = /^[A-Za-z0-9]{27}$/, _n = /^[a-zA-Z0-9_-]{21}$/;
function vn(e) {
	return RegExp(`^[a-zA-Z0-9_-]{${e}}$`);
}
var yn = /^P(?:(\d+W)|(?!.*W)(?=\d|T\d)(\d+Y)?(\d+M)?(\d+D)?(T(?=\d)(\d+H)?(\d+M)?(\d+([.,]\d+)?S)?)?)$/, bn = /^[-+]?P(?!$)(?:(?:[-+]?\d+Y)|(?:[-+]?\d+[.,]\d+Y$))?(?:(?:[-+]?\d+M)|(?:[-+]?\d+[.,]\d+M$))?(?:(?:[-+]?\d+W)|(?:[-+]?\d+[.,]\d+W$))?(?:(?:[-+]?\d+D)|(?:[-+]?\d+[.,]\d+D$))?(?:T(?=[\d+-])(?:(?:[-+]?\d+H)|(?:[-+]?\d+[.,]\d+H$))?(?:(?:[-+]?\d+M)|(?:[-+]?\d+[.,]\d+M$))?(?:[-+]?\d+(?:[.,]\d+)?S)?)??$/, xn = /^([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})$/, Sn = (e) => e ? RegExp(`^([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-${e}[0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12})$`) : /^([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-8][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}|00000000-0000-0000-0000-000000000000|ffffffff-ffff-ffff-ffff-ffffffffffff)$/, Cn = /*@__PURE__*/ Sn(4), wn = /*@__PURE__*/ Sn(6), Tn = /*@__PURE__*/ Sn(7), En = /^(?:[A-Za-z0-9_'+\-]+\.)*[A-Za-z0-9_'+\-]*[A-Za-z0-9_+-]@(?:[A-Za-z0-9][A-Za-z0-9\-]*\.)+[A-Za-z]{2,}$/, Dn = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/, On = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/, kn = /^[^\s@"]{1,64}@[^\s@]{1,255}$/u, An = kn, jn = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/, Mn = "^(?=[\\s\\S]*[\\p{Extended_Pictographic}\\p{Regional_Indicator}\\u20E3])[\\p{Extended_Pictographic}\\p{Emoji_Component}]+$";
function Nn() {
	return new RegExp(Mn, "u");
}
var Pn = /^(?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])$/, Fn = /^(([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:))$/, In = (e) => {
	let t = Ce(e ?? ":");
	return RegExp(`^(?:[0-9A-F]{2}${t}){5}[0-9A-F]{2}$|^(?:[0-9a-f]{2}${t}){5}[0-9a-f]{2}$`);
}, Ln = /^((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\/([0-9]|[1-2][0-9]|3[0-2])$/, Rn = /^(([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:))\/(12[0-8]|1[01][0-9]|[1-9]?[0-9])$/, zn = /^$|^(?:[0-9a-zA-Z+/]{4})*(?:(?:[0-9a-zA-Z+/]{2}==)|(?:[0-9a-zA-Z+/]{3}=))?$/, Bn = /^(?:[A-Za-z0-9_-]{4})*(?:[A-Za-z0-9_-]{2,3})?$/, Vn = /^(?=.{1,253}\.?$)[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[-0-9a-zA-Z]{0,61}[0-9a-zA-Z])?)*\.?$/, Hn = /^(?=.{1,253}$)([a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,63}$/, Un = /^https?$/, Wn = /^\+[1-9]\d{6,14}$/, Gn = /^\d(?:[ -]?\d){11,18}$/, Kn = /^(?:AED|AFN|ALL|AMD|AOA|ARS|AUD|AWG|AZN|BAM|BBD|BDT|BHD|BIF|BMD|BND|BOB|BOV|BRL|BSD|BTN|BWP|BYN|BZD|CAD|CDF|CHE|CHF|CHW|CLF|CLP|CNY|COP|COU|CRC|CUP|CVE|CZK|DJF|DKK|DOP|DZD|EGP|ERN|ETB|EUR|FJD|FKP|GBP|GEL|GHS|GIP|GMD|GNF|GTQ|GYD|HKD|HNL|HTG|HUF|IDR|ILS|INR|IQD|IRR|ISK|JMD|JOD|JPY|KES|KGS|KHR|KMF|KPW|KRW|KWD|KYD|KZT|LAK|LBP|LKR|LRD|LSL|LYD|MAD|MDL|MGA|MKD|MMK|MNT|MOP|MRU|MUR|MVR|MWK|MXN|MXV|MYR|MZN|NAD|NGN|NIO|NOK|NPR|NZD|OMR|PAB|PEN|PGK|PHP|PKR|PLN|PYG|QAR|RON|RSD|RUB|RWF|SAR|SBD|SCR|SDG|SEK|SGD|SHP|SLE|SOS|SRD|SSP|STN|SVC|SYP|SZL|THB|TJS|TMT|TND|TOP|TRY|TTD|TWD|TZS|UAH|UGX|USD|USN|UYI|UYU|UYW|UZS|VED|VES|VND|VUV|WST|XAD|XAF|XAG|XAU|XBA|XBB|XBC|XBD|XCD|XCG|XDR|XOF|XPD|XPF|XPT|XSU|XTS|XUA|XXX|YER|ZAR|ZMW|ZWG)$/, qn = /^[A-Z]{2}(?!00|01|99)\d{2}[A-Z0-9]{11,30}$/, Jn = "(?:(?:\\d\\d[2468][048]|\\d\\d[13579][26]|\\d\\d0[48]|[02468][048]00|[13579][26]00)-02-29|\\d{4}-(?:(?:0[13578]|1[02])-(?:0[1-9]|[12]\\d|3[01])|(?:0[469]|11)-(?:0[1-9]|[12]\\d|30)|(?:02)-(?:0[1-9]|1\\d|2[0-8])))";
function Yn(e) {
	return RegExp(`^${e}$`);
}
var Xn = /*@__PURE__*/ Yn(Jn);
function Zn(e) {
	let t = "(?:[01]\\d|2[0-3]):[0-5]\\d";
	return typeof e.precision == "number" ? e.precision === -1 ? `${t}` : e.precision === 0 ? `${t}:[0-5]\\d` : `${t}:[0-5]\\d\\.\\d{${e.precision}}` : e.seconds ? `${t}:[0-5]\\d(?:\\.\\d+)?` : `${t}(?::[0-5]\\d(?:\\.\\d+)?)?`;
}
function Qn(e) {
	return RegExp(`^${Zn(e)}$`);
}
function $n(e) {
	let t = ["Z"];
	e.offset && t.push("([+-](?:[01]\\d|2[0-3]):[0-5]\\d)");
	let n = `${Zn({
		precision: e.precision,
		seconds: !0
	})}(?:${t.join("|")})`, r = e.local ? `${n}|${Zn({ precision: e.precision })}` : n;
	return RegExp(`^${Jn}T(?:${r})$`);
}
var er = /^[\s\S]{0,}$/, tr = (e) => {
	let t = e ? `[\\s\\S]{${e?.minimum ?? 0},${e?.maximum ?? ""}}` : "[\\s\\S]*";
	return RegExp(`^${t}$`);
}, nr = /^-?\d+n?$/, rr = /^-?\d+$/, ir = /^-?\d+(?:\.\d+)?$/, ar = /^(?:true|false)$/i, or = /^null$/i, sr = /^undefined$/i, cr = /^[^A-Z]*$/, lr = /^[^a-z]*$/, ur = /^[0-9a-fA-F]*$/;
function dr(e, t) {
	return RegExp(`^[A-Za-z0-9+/]{${e}}${t}$`);
}
function fr(e) {
	return RegExp(`^[A-Za-z0-9_-]{${e}}$`);
}
var pr = /^[0-9a-fA-F]{32}$/, mr = /*@__PURE__*/ dr(22, "=="), hr = /*@__PURE__*/ fr(22), gr = /^[0-9a-fA-F]{40}$/, _r = /*@__PURE__*/ dr(27, "="), vr = /*@__PURE__*/ fr(27), yr = /^[0-9a-fA-F]{64}$/, br = /*@__PURE__*/ dr(43, "="), xr = /*@__PURE__*/ fr(43), Sr = /^[0-9a-fA-F]{96}$/, Cr = /*@__PURE__*/ dr(64, ""), wr = /*@__PURE__*/ fr(64), Tr = /^[0-9a-fA-F]{128}$/, Er = /*@__PURE__*/ dr(86, "=="), Dr = /*@__PURE__*/ fr(86), M = /*@__PURE__*/ O("$ZodCheck", (e, t) => {
	var n;
	e._zod ??= {}, e._zod.def = t, (n = e._zod).onattach ?? (n.onattach = []);
}), Or = (e) => {
	let t = e.value;
	return !m(t) && t.size !== void 0;
}, kr = (e) => {
	let t = e.value;
	return !m(t) && t.length !== void 0;
}, Ar = {
	number: "number",
	bigint: "bigint",
	object: "date"
}, jr = /*@__PURE__*/ O("$ZodCheckLessThan", (e, t) => {
	M.init(e, t);
	let n = Ar[typeof t.value];
	e._zod.check = (r) => {
		(t.inclusive ? r.value <= t.value : r.value < t.value) || r.issues.push({
			origin: Ar[typeof r.value] ?? n,
			code: "too_big",
			maximum: typeof t.value == "object" ? t.value.getTime() : t.value,
			input: r.value,
			inclusive: t.inclusive,
			inst: e,
			continue: !t.abort
		});
	};
}), Mr = /*@__PURE__*/ O("$ZodCheckGreaterThan", (e, t) => {
	M.init(e, t);
	let n = Ar[typeof t.value];
	e._zod.check = (r) => {
		(t.inclusive ? r.value >= t.value : r.value > t.value) || r.issues.push({
			origin: Ar[typeof r.value] ?? n,
			code: "too_small",
			minimum: typeof t.value == "object" ? t.value.getTime() : t.value,
			input: r.value,
			inclusive: t.inclusive,
			inst: e,
			continue: !t.abort
		});
	};
}), Nr = /*@__PURE__*/ O("$ZodCheckMultipleOf", (e, t) => {
	M.init(e, t), e._zod.check = (n) => {
		if (typeof n.value != typeof t.value) throw Error("Cannot mix number and bigint in multiple_of check.");
		(typeof n.value == "bigint" ? t.value !== BigInt(0) && n.value % t.value === BigInt(0) : g(n.value, t.value) === 0) || n.issues.push({
			origin: typeof n.value,
			code: "not_multiple_of",
			divisor: t.value,
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
}), Pr = /*@__PURE__*/ O("$ZodCheckNumberFormat", (e, t) => {
	M.init(e, t), t.format = t.format || "float64";
	let n = t.format?.includes("int"), r = n ? "int" : "number", [i, a] = Ee[t.format];
	e._zod.check = (o) => {
		let s = o.value;
		if (n) {
			if (!Number.isInteger(s)) {
				o.issues.push({
					expected: r,
					format: t.format,
					code: "invalid_type",
					continue: !1,
					input: s,
					inst: e
				});
				return;
			}
			if (!Number.isSafeInteger(s)) {
				s > 0 ? o.issues.push({
					input: s,
					code: "too_big",
					maximum: 2 ** 53 - 1,
					note: "Integers must be within the safe integer range.",
					inst: e,
					origin: r,
					inclusive: !0,
					continue: !t.abort
				}) : o.issues.push({
					input: s,
					code: "too_small",
					minimum: -(2 ** 53 - 1),
					note: "Integers must be within the safe integer range.",
					inst: e,
					origin: r,
					inclusive: !0,
					continue: !t.abort
				});
				return;
			}
		}
		s < i && o.issues.push({
			origin: "number",
			input: s,
			code: "too_small",
			minimum: i,
			inclusive: !0,
			inst: e,
			continue: !t.abort
		}), s > a && o.issues.push({
			origin: "number",
			input: s,
			code: "too_big",
			maximum: a,
			inclusive: !0,
			inst: e,
			continue: !t.abort
		});
	};
}), Fr = /*@__PURE__*/ O("$ZodCheckBigIntFormat", (e, t) => {
	M.init(e, t);
	let [n, r] = De[t.format];
	e._zod.check = (i) => {
		let a = i.value;
		a < n && i.issues.push({
			origin: "bigint",
			input: a,
			code: "too_small",
			minimum: n,
			inclusive: !0,
			inst: e,
			continue: !t.abort
		}), a > r && i.issues.push({
			origin: "bigint",
			input: a,
			code: "too_big",
			maximum: r,
			inclusive: !0,
			inst: e,
			continue: !t.abort
		});
	};
}), Ir = /*@__PURE__*/ O("$ZodCheckMaxSize", (e, t) => {
	var n;
	M.init(e, t), (n = e._zod.def).when ?? (n.when = Or), e._zod.check = (n) => {
		let r = n.value;
		r.size <= t.maximum || n.issues.push({
			origin: Ve(r),
			code: "too_big",
			maximum: t.maximum,
			inclusive: !0,
			input: r,
			inst: e,
			continue: !t.abort
		});
	};
}), Lr = /*@__PURE__*/ O("$ZodCheckMinSize", (e, t) => {
	var n;
	M.init(e, t), (n = e._zod.def).when ?? (n.when = Or), e._zod.check = (n) => {
		let r = n.value;
		r.size >= t.minimum || n.issues.push({
			origin: Ve(r),
			code: "too_small",
			minimum: t.minimum,
			inclusive: !0,
			input: r,
			inst: e,
			continue: !t.abort
		});
	};
}), Rr = /*@__PURE__*/ O("$ZodCheckSizeEquals", (e, t) => {
	var n;
	M.init(e, t), (n = e._zod.def).when ?? (n.when = Or), e._zod.check = (n) => {
		let r = n.value, i = r.size;
		if (i === t.size) return;
		let a = i > t.size;
		n.issues.push({
			origin: Ve(r),
			...a ? {
				code: "too_big",
				maximum: t.size
			} : {
				code: "too_small",
				minimum: t.size
			},
			inclusive: !0,
			exact: !0,
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
}), zr = /*@__PURE__*/ O("$ZodCheckMaxLength", (e, t) => {
	var n;
	M.init(e, t), (n = e._zod.def).when ?? (n.when = kr), e._zod.check = (n) => {
		let r = n.value, i = r.length;
		if ((typeof r == "string" && i > t.maximum ? Ue(r) : i) <= t.maximum) return;
		let a = We(r);
		n.issues.push({
			origin: a,
			code: "too_big",
			maximum: t.maximum,
			inclusive: !0,
			input: r,
			inst: e,
			continue: !t.abort
		});
	};
}), Br = /*@__PURE__*/ O("$ZodCheckMinLength", (e, t) => {
	var n;
	M.init(e, t), (n = e._zod.def).when ?? (n.when = kr), e._zod.check = (n) => {
		let r = n.value, i = r.length;
		if ((typeof r == "string" && i >= t.minimum && i < t.minimum * 2 ? Ue(r) : i) >= t.minimum) return;
		let a = We(r);
		n.issues.push({
			origin: a,
			code: "too_small",
			minimum: t.minimum,
			inclusive: !0,
			input: r,
			inst: e,
			continue: !t.abort
		});
	};
}), Vr = /*@__PURE__*/ O("$ZodCheckLengthEquals", (e, t) => {
	var n;
	M.init(e, t), (n = e._zod.def).when ?? (n.when = kr), e._zod.check = (n) => {
		let r = n.value, i = r.length, a = typeof r == "string" && i >= t.length && i <= t.length * 2 ? Ue(r) : i;
		if (a === t.length) return;
		let o = We(r), s = a > t.length;
		n.issues.push({
			origin: o,
			...s ? {
				code: "too_big",
				maximum: t.length
			} : {
				code: "too_small",
				minimum: t.length
			},
			inclusive: !0,
			exact: !0,
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
}), Hr = /*@__PURE__*/ O("$ZodCheckStringFormat", (e, t) => {
	var n, r;
	M.init(e, t), t.pattern ? (n = e._zod).check ?? (n.check = (n) => {
		t.pattern.lastIndex = 0, t.pattern.test(n.value) || n.issues.push({
			origin: "string",
			code: "invalid_format",
			format: t.format,
			input: n.value,
			...t.pattern ? { pattern: t.pattern.toString() } : {},
			inst: e,
			continue: !t.abort
		});
	}) : (r = e._zod).check ?? (r.check = () => {});
}), Ur = /*@__PURE__*/ O("$ZodCheckRegex", (e, t) => {
	Hr.init(e, t), e._zod.check = (n) => {
		t.pattern.lastIndex = 0, t.pattern.test(n.value) || n.issues.push({
			origin: "string",
			code: "invalid_format",
			format: "regex",
			input: n.value,
			pattern: t.pattern.toString(),
			inst: e,
			continue: !t.abort
		});
	};
}), Wr = /*@__PURE__*/ O("$ZodCheckLowerCase", (e, t) => {
	t.pattern ??= cr, Hr.init(e, t);
}), Gr = /*@__PURE__*/ O("$ZodCheckUpperCase", (e, t) => {
	t.pattern ??= lr, Hr.init(e, t);
}), Kr = /*@__PURE__*/ O("$ZodCheckIncludes", (e, t) => {
	M.init(e, t);
	let n = Ce(t.includes);
	t.pattern = new RegExp(typeof t.position == "number" ? `^.{${t.position},}${n}` : n), e._zod.check = (n) => {
		n.value.includes(t.includes, t.position) || n.issues.push({
			origin: "string",
			code: "invalid_format",
			format: "includes",
			includes: t.includes,
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
}), qr = /*@__PURE__*/ O("$ZodCheckStartsWith", (e, t) => {
	M.init(e, t);
	let n = RegExp(`^${Ce(t.prefix)}.*`);
	t.pattern ??= n, e._zod.check = (n) => {
		n.value.startsWith(t.prefix) || n.issues.push({
			origin: "string",
			code: "invalid_format",
			format: "starts_with",
			prefix: t.prefix,
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
}), Jr = /*@__PURE__*/ O("$ZodCheckEndsWith", (e, t) => {
	M.init(e, t);
	let n = RegExp(`.*${Ce(t.suffix)}$`);
	t.pattern ??= n, e._zod.check = (n) => {
		n.value.endsWith(t.suffix) || n.issues.push({
			origin: "string",
			code: "invalid_format",
			format: "ends_with",
			suffix: t.suffix,
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
});
function Yr(e, t, n) {
	e.issues.length && t.issues.push(...w(n, e.issues));
}
var Xr = /*@__PURE__*/ O("$ZodCheckProperty", (e, t) => {
	M.init(e, t), e._zod.check = (e) => {
		let n = t.schema._zod.run({
			value: e.value[t.property],
			issues: []
		}, {});
		if (n instanceof Promise) return n.then((n) => Yr(n, e, t.property));
		Yr(n, e, t.property);
	};
}), Zr = /*@__PURE__*/ O("$ZodCheckProperties", (e, t) => {
	M.init(e, t), nt(e, Symbol.iterator, function* () {
		yield e;
	});
	let n;
	e._zod.check = (r) => {
		if (r.value == null) {
			r.issues.push({
				expected: "object",
				code: "invalid_type",
				input: r.value,
				inst: e
			});
			return;
		}
		n ??= Reflect.ownKeys(t.shape).map((e) => [e, t.shape[e]]);
		let i = r.value, a;
		for (let [e, t] of n) {
			let n = t._zod.run({
				value: i[e],
				issues: []
			}, {});
			n instanceof Promise ? (a ??= [], a.push(n.then((t) => Yr(t, r, e)))) : Yr(n, r, e);
		}
		if (a) return Promise.all(a).then(() => void 0);
	};
}), Qr = /*@__PURE__*/ O("$ZodCheckMimeType", (e, t) => {
	M.init(e, t);
	let n = new Set(t.mime);
	e._zod.check = (r) => {
		n.has(r.value.type) || r.issues.push({
			code: "invalid_value",
			values: t.mime,
			input: r.value.type,
			inst: e,
			continue: !t.abort
		});
	};
}), $r = /*@__PURE__*/ O("$ZodCheckOverwrite", (e, t) => {
	M.init(e, t), e._zod.check = (e) => {
		e.value = t.tx(e.value);
	};
}), ei = class {
	constructor(e = [], t = {}) {
		this.content = [], this.indent = 0, this.args = e, this.closed = t;
	}
	indented(e) {
		this.indent += 1;
		try {
			e(this);
		} finally {
			--this.indent;
		}
	}
	write(e) {
		if (typeof e == "function") {
			e(this, { execution: "sync" }), e(this, { execution: "async" });
			return;
		}
		let t = e.split("\n").filter((e) => e), n = Math.min(...t.map((e) => e.length - e.trimStart().length)), r = t.map((e) => e.slice(n)).map((e) => " ".repeat(this.indent * 2) + e);
		for (let e of r) this.content.push(e);
	}
	compile() {
		let e = Function, t = this?.content ?? [""];
		return new e(...Object.keys(this.closed), `return function (${this.args.join(", ")}) {\n${t.join("\n")}\n};`)(...Object.values(this.closed));
	}
}, ti = {
	major: 4,
	minor: 6,
	patch: 7
}, N = /*@__PURE__*/ O("$ZodType", (e, t) => {
	var n;
	e ??= {}, e._zod.def = t, e._zod.bag = e._zod.bag || {}, e._zod.version = ti;
	let r = e._zod.def.checks, i = e._zod.traits.has("$ZodCheck") ? [e, ...r ?? []] : r?.length ? [...r] : [];
	for (let t of i) for (let n of t._zod.onattach) n(e);
	if (i.length === 0) (n = e._zod).deferred ?? (n.deferred = []), e._zod.deferred?.push(() => {
		e._zod.run = e._zod.parse;
	});
	else {
		let t = (t, n, r) => {
			if (t.memo) return t;
			let i = C(t), a;
			for (let o of n) {
				if (o._zod.def.when) {
					if (Le(t) || !o._zod.def.when(t)) continue;
				} else if (i) continue;
				let n = t.issues.length, s = o._zod.check(t);
				if (s instanceof Promise && r?.async === !1) throw new vt();
				if (a || s instanceof Promise) a = (a ?? Promise.resolve()).then(async () => {
					await s, t.issues.length !== n && (Be(t.issues, n, e), i ||= C(t, n));
				});
				else {
					if (t.issues.length === n) continue;
					Be(t.issues, n, e), i ||= C(t, n);
				}
			}
			return a ? a.then(() => t) : t;
		}, n = (n, r, a) => {
			if (C(n)) return n.aborted = !0, n;
			let o = t(r, i, a);
			if (o instanceof Promise) {
				if (a.async === !1) throw new vt();
				return o.then((t) => e._zod.parse(t, a));
			}
			return e._zod.parse(o, a);
		};
		e._zod.run = (r, a) => {
			if (a.skipChecks) return e._zod.parse(r, a);
			if (a.direction === "backward") {
				let t = e._zod.parse({
					value: r.value,
					issues: []
				}, {
					...a,
					skipChecks: !0
				});
				return t instanceof Promise ? t.then((e) => n(e, r, a)) : n(t, r, a);
			}
			let o = e._zod.parse(r, a);
			if (o instanceof Promise) {
				if (a.async === !1) throw new vt();
				return o.then((e) => t(e, i, a));
			}
			return t(o, i, a);
		};
	}
}, {
	get "~standard"() {
		return nt(this, "~standard", ii(this));
	},
	set "~standard"(e) {
		tt(this, "~standard", e);
	}
}), ni = (e, t) => e.issues.length ? { issues: e.issues.map((e) => T(e, t, A())) } : { value: e.value };
async function ri(e, t) {
	let n = { async: !0 };
	return ni(await e._zod.run({
		value: t,
		issues: []
	}, n), n);
}
function ii(e) {
	return {
		validate: (t) => {
			let n = { async: !1 };
			try {
				let r = e._zod.run({
					value: t,
					issues: []
				}, n);
				if (!(r instanceof Promise)) return ni(r, n);
			} catch {}
			return ri(e, t);
		},
		vendor: "zod",
		version: 1
	};
}
var ai = /*@__PURE__*/ O("$ZodString", (e, t) => {
	N.init(e, t), e._zod.pattern = t.pattern ?? er, e._zod.parse = (n, r) => {
		if (t.coerce) try {
			n.value = String(n.value);
		} catch {}
		return typeof n.value == "string" || n.issues.push({
			expected: "string",
			code: "invalid_type",
			input: n.value,
			inst: e
		}), n;
	};
}), P = /*@__PURE__*/ O("$ZodStringFormat", (e, t) => {
	Hr.init(e, t), ai.init(e, t);
}), oi = /*@__PURE__*/ O("$ZodGUID", (e, t) => {
	t.pattern ??= xn, P.init(e, t);
}), si = /*@__PURE__*/ O("$ZodUUID", (e, t) => {
	if (t.version) {
		let e = {
			v1: 1,
			v2: 2,
			v3: 3,
			v4: 4,
			v5: 5,
			v6: 6,
			v7: 7,
			v8: 8
		}[t.version];
		if (e === void 0) throw Error(`Invalid UUID version: "${t.version}"`);
		t.pattern ??= Sn(e);
	} else t.pattern ??= Sn();
	P.init(e, t);
}), ci = /*@__PURE__*/ O("$ZodEmail", (e, t) => {
	t.pattern ??= En, P.init(e, t);
});
function li(e) {
	try {
		return typeof URL < "u" && typeof URL.canParse == "function" ? URL.canParse(e) : (new URL(e), !0);
	} catch {
		return !1;
	}
}
function ui(e, t) {
	return !("normalize" in t) && !("hostname" in t) && !("protocol" in t) ? li(e) || 2 : di(e, t);
}
function di(e, t) {
	if (!t.normalize && t.protocol?.source === Un.source && !/^https?:\/\//i.test(e)) return 1;
	try {
		if (typeof URL < "u") {
			let t = URL;
			if (typeof t.parse == "function") return t.parse(e) ?? 2;
		}
		return new URL(e);
	} catch {
		return 2;
	}
}
var fi = /[\t\n\r]/g;
function pi(e) {
	return e.replace(fi, "");
}
function mi(e, t) {
	return t.lastIndex = 0, t.test(e.hostname);
}
function hi(e, t) {
	return t.lastIndex = 0, t.test(e.protocol.endsWith(":") ? e.protocol.slice(0, -1) : e.protocol);
}
var gi = /*@__PURE__*/ O("$ZodURL", (e, t) => {
	P.init(e, t), e._zod.check = (n) => {
		try {
			let r = n.value.trim(), i = ui(r, t);
			if (i === 1) {
				n.issues.push({
					code: "invalid_format",
					format: "url",
					note: "Invalid URL format",
					input: n.value,
					inst: e,
					continue: !t.abort
				});
				return;
			}
			if (i === 2) {
				n.issues.push({
					code: "invalid_format",
					format: "url",
					input: n.value,
					inst: e,
					continue: !t.abort
				});
				return;
			}
			if (i === !0) {
				n.value = pi(r);
				return;
			}
			t.hostname && !mi(i, t.hostname) && n.issues.push({
				code: "invalid_format",
				format: "url",
				note: "Invalid hostname",
				pattern: t.hostname.source,
				input: n.value,
				inst: e,
				continue: !t.abort
			}), t.protocol && !hi(i, t.protocol) && n.issues.push({
				code: "invalid_format",
				format: "url",
				note: "Invalid protocol",
				pattern: t.protocol.source,
				input: n.value,
				inst: e,
				continue: !t.abort
			}), n.value = t.normalize ? i.href : pi(r);
			return;
		} catch {
			n.issues.push({
				code: "invalid_format",
				format: "url",
				input: n.value,
				inst: e,
				continue: !t.abort
			});
		}
	};
}), _i = /*@__PURE__*/ O("$ZodEmoji", (e, t) => {
	t.pattern ??= Nn(), P.init(e, t);
}), vi = /*@__PURE__*/ O("$ZodNanoID", (e, t) => {
	if (t.length !== void 0 && (!Number.isInteger(t.length) || t.length < 1)) throw Error(`Invalid nanoid length: ${t.length}`);
	t.pattern ??= t.length === void 0 ? _n : vn(t.length), P.init(e, t);
}), yi = /*@__PURE__*/ O("$ZodCUID", (e, t) => {
	t.pattern ??= fn, P.init(e, t);
}), bi = /*@__PURE__*/ O("$ZodCUID2", (e, t) => {
	t.pattern ??= pn, P.init(e, t);
}), xi = /*@__PURE__*/ O("$ZodULID", (e, t) => {
	t.pattern ??= mn, P.init(e, t);
}), Si = /*@__PURE__*/ O("$ZodXID", (e, t) => {
	t.pattern ??= hn, P.init(e, t);
}), Ci = /*@__PURE__*/ O("$ZodKSUID", (e, t) => {
	t.pattern ??= gn, P.init(e, t);
}), wi = /*@__PURE__*/ O("$ZodISODateTime", (e, t) => {
	t.pattern ??= $n(t), P.init(e, t);
}), Ti = /*@__PURE__*/ O("$ZodISODate", (e, t) => {
	t.pattern ??= Xn, P.init(e, t);
}), Ei = /*@__PURE__*/ O("$ZodISOTime", (e, t) => {
	t.pattern ??= Qn(t), P.init(e, t);
}), Di = /*@__PURE__*/ O("$ZodISODuration", (e, t) => {
	t.pattern ??= yn, P.init(e, t);
}), Oi = /*@__PURE__*/ O("$ZodIPv4", (e, t) => {
	t.pattern ??= Pn, P.init(e, t);
}), ki = /^[0-9a-fA-F:.]+$/;
function Ai(e) {
	return ki.test(e) ? li(`http://[${e}]`) : !1;
}
var ji = /*@__PURE__*/ O("$ZodIPv6", (e, t) => {
	t.pattern ??= Fn, P.init(e, t), e._zod.check = (n) => {
		Ai(n.value) || n.issues.push({
			code: "invalid_format",
			format: "ipv6",
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
}), Mi = /*@__PURE__*/ O("$ZodMAC", (e, t) => {
	t.pattern ??= In(t.delimiter), P.init(e, t);
}), Ni = /*@__PURE__*/ O("$ZodCIDRv4", (e, t) => {
	t.pattern ??= Ln, P.init(e, t);
});
function Pi(e) {
	let t = e.split("/");
	if (t.length !== 2) return !1;
	let [n, r] = t;
	if (!r) return !1;
	let i = Number(r);
	return `${i}` !== r || i < 0 || i > 128 ? !1 : Ai(n);
}
var Fi = /*@__PURE__*/ O("$ZodCIDRv6", (e, t) => {
	t.pattern ??= Rn, P.init(e, t), e._zod.check = (n) => {
		Pi(n.value) || n.issues.push({
			code: "invalid_format",
			format: "cidrv6",
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
});
function Ii(e) {
	if (e === "") return !0;
	if (/\s/.test(e) || e.length % 4 != 0) return !1;
	try {
		return atob(e), !0;
	} catch {
		return !1;
	}
}
var Li = /^[0-9a-zA-Z+/]*={0,2}$/, Ri = /*@__PURE__*/ O("$ZodBase64", (e, t) => {
	t.pattern ??= Li, P.init(e, t), e._zod.check = (n) => {
		Ii(n.value) || n.issues.push({
			code: "invalid_format",
			format: "base64",
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
}), zi = /^[A-Za-z0-9_-]*$/;
function Bi(e) {
	if (!zi.test(e)) return !1;
	let t = e.replace(/[-_]/g, (e) => e === "-" ? "+" : "/");
	return Ii(t.padEnd(Math.ceil(t.length / 4) * 4, "="));
}
var Vi = /*@__PURE__*/ O("$ZodBase64URL", (e, t) => {
	t.pattern ??= zi, P.init(e, t), e._zod.check = (n) => {
		Bi(n.value) || n.issues.push({
			code: "invalid_format",
			format: "base64url",
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
}), Hi = /*@__PURE__*/ O("$ZodE164", (e, t) => {
	t.pattern ??= Wn, P.init(e, t);
}), Ui = /[- ]/g;
function Wi(e) {
	let t = e.length, n = 1, r = 0;
	for (; t;) {
		let i = e.charCodeAt(--t) - 48;
		n ^= 1, r += n ? [
			0,
			2,
			4,
			6,
			8,
			1,
			3,
			5,
			7,
			9
		][i] : i;
	}
	return r % 10 == 0;
}
function Gi(e) {
	return Gn.test(e) ? Wi(e.replace(Ui, "")) : !1;
}
var Ki = /*@__PURE__*/ O("$ZodCreditCard", (e, t) => {
	t.pattern ??= Gn, P.init(e, t), e._zod.check = (n) => {
		Gi(n.value) || n.issues.push({
			code: "invalid_format",
			format: "credit_card",
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
});
function qi(e) {
	let t = 0, n = e.length;
	for (let r = 4; r < n; r++) {
		let n = e.charCodeAt(r);
		t = (n >= 65 ? t * 100 + (n - 55) : t * 10 + (n - 48)) % 97;
	}
	for (let n = 0; n < 4; n++) {
		let r = e.charCodeAt(n);
		t = (r >= 65 ? t * 100 + (r - 55) : t * 10 + (r - 48)) % 97;
	}
	return t === 1;
}
function Ji(e) {
	return qn.test(e) ? qi(e) : !1;
}
var Yi = /*@__PURE__*/ O("$ZodIBAN", (e, t) => {
	t.pattern ??= qn, P.init(e, t), e._zod.check = (n) => {
		Ji(n.value) || n.issues.push({
			code: "invalid_format",
			format: "iban",
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
});
function Xi(e, t = null) {
	try {
		let n = e.split(".");
		if (n.length !== 3) return !1;
		let [r] = n;
		if (!r) return !1;
		let i = JSON.parse(atob(r));
		return !("typ" in i && i?.typ !== "JWT" || !i.alg || t && (!("alg" in i) || i.alg !== t));
	} catch {
		return !1;
	}
}
var Zi = /*@__PURE__*/ O("$ZodJWT", (e, t) => {
	P.init(e, t), e._zod.check = (n) => {
		Xi(n.value, t.alg) || n.issues.push({
			code: "invalid_format",
			format: "jwt",
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
}), Qi = /*@__PURE__*/ O("$ZodCustomStringFormat", (e, t) => {
	P.init(e, t), e._zod.check = (n) => {
		t.fn(n.value) || n.issues.push({
			code: "invalid_format",
			format: t.format,
			input: n.value,
			inst: e,
			continue: !t.abort
		});
	};
}), $i = /*@__PURE__*/ O("$ZodNumber", (e, t) => {
	N.init(e, t), e._zod.pattern = ir, e._zod.parse = (n, r) => {
		if (t.coerce) try {
			n.value = Number(n.value);
		} catch {}
		let i = n.value;
		if (typeof i == "number" && !Number.isNaN(i) && Number.isFinite(i)) return n;
		let a = typeof i == "number" ? Number.isNaN(i) ? "NaN" : Number.isFinite(i) ? void 0 : String(i) : void 0;
		return n.issues.push({
			expected: "number",
			code: "invalid_type",
			input: i,
			inst: e,
			...a ? { received: a } : {}
		}), n;
	};
}), ea = /*@__PURE__*/ O("$ZodNumberFormat", (e, t) => {
	Pr.init(e, t), $i.init(e, t);
}), ta = /*@__PURE__*/ O("$ZodBoolean", (e, t) => {
	N.init(e, t), e._zod.pattern = ar, e._zod.parse = (n, r) => {
		if (t.coerce) try {
			n.value = !!n.value;
		} catch {}
		let i = n.value;
		return typeof i == "boolean" || n.issues.push({
			expected: "boolean",
			code: "invalid_type",
			input: i,
			inst: e
		}), n;
	};
}), na = /*@__PURE__*/ O("$ZodBigInt", (e, t) => {
	N.init(e, t), e._zod.pattern = nr, e._zod.parse = (n, r) => {
		if (t.coerce) try {
			n.value = BigInt(n.value);
		} catch {}
		return typeof n.value == "bigint" || n.issues.push({
			expected: "bigint",
			code: "invalid_type",
			input: n.value,
			inst: e
		}), n;
	};
}), ra = /*@__PURE__*/ O("$ZodBigIntFormat", (e, t) => {
	Fr.init(e, t), na.init(e, t);
}), ia = /*@__PURE__*/ O("$ZodSymbol", (e, t) => {
	N.init(e, t), e._zod.parse = (t, n) => {
		let r = t.value;
		return typeof r == "symbol" || t.issues.push({
			expected: "symbol",
			code: "invalid_type",
			input: r,
			inst: e
		}), t;
	};
}), aa = /*@__PURE__*/ O("$ZodUndefined", (e, t) => {
	N.init(e, t), e._zod.pattern = sr, e._zod.values = /* @__PURE__ */ new Set([void 0]), e._zod.parse = (t, n) => {
		let r = t.value;
		return r === void 0 || t.issues.push({
			expected: "undefined",
			code: "invalid_type",
			input: r,
			inst: e
		}), t;
	};
}), oa = /*@__PURE__*/ O("$ZodNull", (e, t) => {
	N.init(e, t), e._zod.pattern = or, e._zod.values = /* @__PURE__ */ new Set([null]), e._zod.parse = (t, n) => {
		let r = t.value;
		return r === null || t.issues.push({
			expected: "null",
			code: "invalid_type",
			input: r,
			inst: e
		}), t;
	};
}), sa = /*@__PURE__*/ O("$ZodAny", (e, t) => {
	N.init(e, t), e._zod.parse = (e) => e;
}), ca = /*@__PURE__*/ O("$ZodUnknown", (e, t) => {
	N.init(e, t), e._zod.parse = (e) => e;
}), la = /*@__PURE__*/ O("$ZodNever", (e, t) => {
	N.init(e, t), e._zod.parse = (t, n) => (t.issues.push({
		expected: "never",
		code: "invalid_type",
		input: t.value,
		inst: e
	}), t);
}), ua = /*@__PURE__*/ O("$ZodVoid", (e, t) => {
	N.init(e, t), e._zod.parse = (t, n) => {
		let r = t.value;
		return r === void 0 || t.issues.push({
			expected: "void",
			code: "invalid_type",
			input: r,
			inst: e
		}), t;
	};
}), da = /*@__PURE__*/ O("$ZodDate", (e, t) => {
	N.init(e, t), e._zod.parse = (n, r) => {
		if (t.coerce) try {
			n.value = new Date(n.value);
		} catch {}
		let i = n.value, a = i instanceof Date;
		return a && !Number.isNaN(i.getTime()) || n.issues.push({
			expected: "date",
			code: "invalid_type",
			input: i,
			...a ? { received: "Invalid Date" } : {},
			inst: e
		}), n;
	};
});
function fa(e, t) {
	let n = ze(e._zod.def.checks);
	if (!n.length) return;
	let r = Math.min(...n.map((e) => e[0])), i = e._zod.parse;
	e._zod.parse = (n, a) => {
		let o = t(n.value);
		if (o > r && !a?.skipChecks && a?.direction !== "backward") {
			for (let [t, r] of ze(e._zod.def.checks)) if (o > t && r._zod.def.when(n)) {
				let t = n.issues.length;
				return r._zod.check(n), n.issues[t].continue = !1, Be(n.issues, t, e), n;
			}
		}
		return i(n, a);
	};
}
var pa = (e) => Array.isArray(e) ? e.length : -1, ma = (e) => e instanceof Map ? e.size : -1, ha = (e) => e instanceof Set ? e.size : -1;
function ga(e, t, n) {
	e.issues.length && t.issues.push(...w(n, e.issues)), t.value[n] = e.value;
}
var _a = /*@__PURE__*/ O("$ZodArray", (e, t) => {
	N.init(e, t);
	let n = k.memoizer;
	n?.attach(e), e._zod.parse = (r, i) => {
		let a = r.value;
		if (!Array.isArray(a)) return r.issues.push({
			expected: "array",
			code: "invalid_type",
			input: a,
			inst: e
		}), r;
		r.value = n ? n.alloc(e, r, Array(a.length), i) : Array(a.length);
		let o = [], s = i?.abortEarly;
		for (let e = 0; e < a.length; e++) {
			let n = a[e], c = t.element._zod.run({
				value: n,
				issues: []
			}, i);
			if (c instanceof Promise) o.push(c.then((t) => ga(t, r, e)));
			else if (ga(c, r, e), s && c.issues.length !== 0 && C(c)) break;
		}
		return o.length ? Promise.all(o).then(() => r) : r;
	}, fa(e, pa);
});
function va(e, t, n, r, i, a) {
	let o = n in r, s = a === "optional";
	if (o || !s || i !== "optional") {
		if (e.issues.length) {
			if (i !== void 0 && s && !o) return;
			t.issues.push(...w(n, e.issues));
		}
		!o && i === void 0 ? e.issues.length || t.issues.push({
			code: "invalid_type",
			expected: "nonoptional",
			input: void 0,
			path: [n]
		}) : e.value === void 0 ? (o || i === "defaulted" && !s) && (t.value[n] = void 0) : t.value[n] = e.value;
	}
}
var ya = [];
function ba(e) {
	let t = Object.keys(e.shape), n = Object.getOwnPropertySymbols(e.shape), r = n.length ? n : ya, i = r.length ? [...t, ...r] : t;
	for (let t of i) if (!e.shape?.[t]?._zod?.traits?.has("$ZodType")) throw Error(`Invalid element at key "${String(t)}": expected a Zod schema`);
	let a = Te(e.shape);
	return {
		...e,
		allKeys: i,
		symbolKeys: r,
		keySet: new Set(t),
		numKeys: t.length,
		optionalKeys: new Set(a)
	};
}
function xa(e, t, n, r, i, a, o) {
	let s = [], c = i.keySet, l = i.catchall._zod, u = l.def.type, d = l.optin, f = l.optout, p = 0;
	for (let i in t) {
		if (o && n.issues.length !== p) {
			if (C(n, p)) break;
			p = n.issues.length;
		}
		if (c.has(i)) continue;
		if (i === "__proto__") {
			u === "never" && s.push(i);
			continue;
		}
		if (u === "never") {
			s.push(i);
			continue;
		}
		let a = l.run({
			value: t[i],
			issues: []
		}, r);
		a instanceof Promise ? e.push(a.then((e) => va(e, n, i, t, d, f))) : va(a, n, i, t, d, f);
	}
	return s.length && n.issues.push({
		code: "unrecognized_keys",
		keys: s,
		input: t,
		inst: a,
		continue: !0
	}), e.length ? Promise.all(e).then(() => n) : n;
}
var Sa = /*@__PURE__*/ O("$ZodObject", (e, t) => {
	N.init(e, t);
	let n = Object.getOwnPropertyDescriptor(t, "shape"), r = n?.get ? n.get.raw : t.shape ?? {};
	if (r) {
		let e = () => {
			let n = { ...r };
			return Object.defineProperty(t, "shape", { value: n }), e.raw = n, n;
		};
		e.raw = r, Object.defineProperty(t, "shape", { get: e });
	}
	let i = p(() => ba(t));
	D(e, "propValues", (e) => {
		let t = e.def.shape, n = {};
		for (let e in t) {
			let r = t[e]._zod;
			if (r.values) {
				Object.prototype.hasOwnProperty.call(n, e) || _(n, e, /* @__PURE__ */ new Set());
				for (let t of r.values) n[e].add(t);
				r.optin !== void 0 && n[e].add(void 0);
			}
		}
		return n;
	});
	let a = he, o = t.catchall, s, c = k.memoizer;
	c?.attach(e), e._zod.parse = (t, n) => {
		s ??= i.value;
		let r = t.value;
		if (!a(r)) return t.issues.push({
			expected: "object",
			code: "invalid_type",
			input: r,
			inst: e
		}), t;
		t.value = c ? c.alloc(e, t, {}, n) : {};
		let l = [], u = s.shape, d = n?.abortEarly, f = t.issues.length;
		for (let e of s.allKeys) {
			if (d && t.issues.length !== f) {
				if (C(t, f)) break;
				f = t.issues.length;
			}
			if (e === "__proto__") continue;
			let i = u[e], a = i._zod.optin, o = i._zod.optout, s = i._zod.run({
				value: r[e],
				issues: []
			}, n);
			s instanceof Promise ? l.push(s.then((n) => va(n, t, e, r, a, o))) : va(s, t, e, r, a, o);
		}
		return o ? xa(l, r, t, n, i.value, e, d === !0) : l.length ? Promise.all(l).then(() => t) : t;
	};
}), Ca = /*@__PURE__*/ O("$ZodObjectJIT", (e, t) => {
	Sa.init(e, t);
	let n = e._zod.parse, r = p(() => ba(t)), i = k.memoizer, a = (t) => {
		let n = r.value, a = n.symbolKeys, o = new ei(["payload", "ctx"], {
			shape: t,
			inst: e,
			memo: i,
			syms: a
		}), s = (e) => `shape[${e}]._zod.run({ value: input[${e}], issues: [] }, ctx)`, c = (e, t) => `
          let ${e}_ab = false;
          for (let i = 0; i < ${e}.issues.length; i++) {
            const iss = ${e}.issues[i];
            iss.path = iss.path ? [${t}, ...iss.path] : [${t}];
            payload.issues.push(iss);
            if (iss.continue !== true) ${e}_ab = true;
          }
          if (${e}_ab && ctx && ctx.abortEarly) {
            payload.value = newResult;
            return payload;
          }`;
		o.write("const input = payload.value;");
		let l = Object.create(null), u = 0;
		for (let e of n.allKeys) l[e] = `key_${u++}`;
		o.write(i ? "const newResult = memo.alloc(inst, payload, {}, ctx);" : "const newResult = {};");
		for (let e of n.allKeys) {
			if (e === "__proto__") continue;
			let n = l[e], r = typeof e == "symbol" ? `syms[${a.indexOf(e)}]` : y(e), i = `${r} in input`, u = t[e], d = u?._zod?.optin, f = d !== void 0, p = u?._zod?.optout === "optional";
			if (o.write(`const ${n} = ${s(r)};`), f && p) {
				let e = d === "optional" ? `${n}_present` : `${n}.value !== undefined || ${n}_present`;
				o.write(`
        const ${n}_present = ${i};
        if (!${n}.issues.length || ${n}_present) {
          if (${n}.issues.length) {${c(n, r)}
          }

          if (${e}) {
            newResult[${r}] = ${n}.value;
          }
        }

      `);
			} else f ? (o.write(`
        if (${n}.issues.length) {${c(n, r)}
        }
      `), d === "defaulted" ? o.write(`newResult[${r}] = ${n}.value;`) : o.write(`
        if (${n}.value !== undefined || ${i}) {
          newResult[${r}] = ${n}.value;
        }
      `)) : o.write(`
        const ${n}_present = ${i};
        if (${n}.issues.length) {${c(n, r)}
        }
        if (!${n}_present && !${n}.issues.length) {
          payload.issues.push({
            code: "invalid_type",
            expected: "nonoptional",
            input: undefined,
            path: [${r}]
          });
          if (ctx && ctx.abortEarly) {
            payload.value = newResult;
            return payload;
          }
        }

        if (${n}_present) {
          newResult[${r}] = ${n}.value;
        }

      `);
		}
		return o.write("payload.value = newResult;"), o.write("return payload;"), o.compile();
	}, o, s = he, c = !k.jitless, l = c && ge.value, u = t.catchall, d;
	e._zod.parse = (i, f) => {
		d ??= r.value;
		let p = i.value;
		return s(p) ? c && l && f?.async === !1 && f.jitless !== !0 ? (o ||= a(t.shape), i = o(i, f), u ? xa([], p, i, f, d, e, f?.abortEarly === !0) : i) : n(i, f) : (i.issues.push({
			expected: "object",
			code: "invalid_type",
			input: p,
			inst: e
		}), i);
	};
});
function wa(e, t, n, r) {
	for (let n of e) if (n.issues.length === 0) return t.value = n.value, t;
	let i = e.filter((e) => !C(e));
	return i.length === 1 ? (t.value = i[0].value, i[0]) : (t.issues.push({
		code: "invalid_union",
		input: t.value,
		inst: n,
		errors: e.map((e) => e.issues.map((e) => T(e, r, A())))
	}), t);
}
var Ta = /*@__PURE__*/ O("$ZodUnion", (e, t) => {
	N.init(e, t), D(e, "optin", (e) => e.def.options.some((e) => e._zod.optin === "defaulted") ? "defaulted" : e.def.options.some((e) => e._zod.optin !== void 0) ? "optional" : void 0), D(e, "optout", (e) => e.def.options.some((e) => e._zod.optout === "optional") ? "optional" : void 0), D(e, "values", (e) => {
		if (e.def.options.every((e) => e._zod.values)) return new Set(e.def.options.flatMap((e) => Array.from(e._zod.values)));
	}), D(e, "pattern", (e) => {
		if (e.def.options.every((e) => e._zod.pattern)) {
			let t = e.def.options.map((e) => e._zod.pattern);
			return RegExp(`^(${t.map((e) => h(e.source)).join("|")})$`);
		}
	});
	let n = t.options.length === 1 ? t.options[0]._zod.run : null;
	e._zod.parse = (r, i) => {
		if (n) return n(r, i);
		let a = !1, o = [];
		for (let e of t.options) {
			let t = e._zod.run({
				value: r.value,
				issues: []
			}, i);
			if (t instanceof Promise) o.push(t), a = !0;
			else {
				if (t.issues.length === 0) return t;
				o.push(t);
			}
		}
		return a ? Promise.all(o).then((t) => wa(t, r, e, i)) : wa(o, r, e, i);
	};
});
function Ea(e, t, n, r) {
	let i = [];
	for (let t = 0; t < e.length; t++) e[t].issues.length === 0 && i.push(t);
	return i.length === 1 ? (t.value = e[i[0]].value, t) : (i.length === 0 ? t.issues.push({
		code: "invalid_union",
		input: t.value,
		inst: n,
		errors: e.map((e) => e.issues.map((e) => T(e, r, A())))
	}) : t.issues.push({
		code: "invalid_union",
		input: t.value,
		inst: n,
		errors: [],
		inclusive: !1,
		matches: i
	}), t);
}
var Da = /*@__PURE__*/ O("$ZodXor", (e, t) => {
	Ta.init(e, t), t.inclusive = !1;
	let n = t.options.length === 1 ? t.options[0]._zod.run : null;
	e._zod.parse = (r, i) => {
		if (n) return n(r, i);
		let a = !1, o = [];
		for (let e of t.options) {
			let t = e._zod.run({
				value: r.value,
				issues: []
			}, i);
			t instanceof Promise ? (o.push(t), a = !0) : o.push(t);
		}
		return a ? Promise.all(o).then((t) => Ea(t, r, e, i)) : Ea(o, r, e, i);
	};
});
function Oa(e, t) {
	let n = e._zod, r = n.bag.optionsMap;
	r || (r = ka(n.def), n.bag.optionsMap = r);
	let i = r.get(t);
	if (i === null) throw Error(`Ambiguous discriminator value "${String(t)}"`);
	return i;
}
function ka(e) {
	let t = /* @__PURE__ */ new Map();
	for (let n of e.options) {
		let r = n._zod.propValues?.[e.discriminator];
		if (!r || r.size === 0) throw Error(`Invalid discriminated union option at index "${e.options.indexOf(n)}"`);
		for (let e of r) if (t.has(e)) {
			if (e !== void 0) throw Error(`Duplicate discriminator value "${String(e)}"`);
			t.set(e, null);
		} else t.set(e, n);
	}
	return t;
}
var Aa = /*@__PURE__*/ O("$ZodDiscriminatedUnion", (e, t) => {
	t.inclusive = !1, Ta.init(e, t);
	let n = e._zod.parse;
	D(e, "propValues", (e) => {
		let t = {}, n = 0;
		for (let r of e.def.options) {
			let i = r._zod.propValues;
			if (!i || Object.keys(i).length === 0) throw Error(`Invalid discriminated union option at index "${e.def.options.indexOf(r)}"`);
			i[e.def.discriminator]?.has(void 0) && n++;
			for (let [e, n] of Object.entries(i)) {
				Object.prototype.hasOwnProperty.call(t, e) || _(t, e, /* @__PURE__ */ new Set());
				for (let r of n) t[e].add(r);
			}
		}
		return !e.def.unionFallback && n > 1 && t[e.def.discriminator]?.delete(void 0), t;
	}), t.options.forEach((e, n) => {
		let r = re(e._zod.def);
		if (r && !Object.prototype.hasOwnProperty.call(r, t.discriminator)) throw Error(`Invalid discriminated union option at index "${n}"`);
	});
	let r = p(() => ka(t));
	e._zod.parse = (i, a) => {
		let o = i.value;
		if (!he(o)) return i.issues.push({
			code: "invalid_type",
			expected: "object",
			input: o,
			inst: e
		}), i;
		let s = o?.[t.discriminator], c = r.value.get(s);
		return c && (s !== void 0 || a.direction !== "backward") ? c._zod.run(i, a) : t.unionFallback || a.direction === "backward" ? n(i, a) : (i.issues.push({
			code: "invalid_union",
			errors: [],
			note: "No matching discriminator",
			discriminator: t.discriminator,
			options: Array.from(r.value.keys()).filter((e) => r.value.get(e) !== null),
			input: o,
			path: [t.discriminator],
			inst: e
		}), i);
	};
}), ja = /*@__PURE__*/ O("$ZodIntersection", (e, t) => {
	N.init(e, t), e._zod.parse = (e, n) => {
		let r = e.value, i = t.left._zod.run({
			value: r,
			issues: []
		}, n), a = t.right._zod.run({
			value: r,
			issues: []
		}, n);
		return i instanceof Promise || a instanceof Promise ? Promise.all([i, a]).then(([t, n]) => Na(e, t, n)) : Na(e, i, a);
	};
});
function Ma(e, t) {
	if (e === t || e instanceof Date && t instanceof Date && +e == +t) return {
		valid: !0,
		data: e
	};
	if (_e(e) && _e(t)) {
		let n = Object.keys(t), r = Object.keys(e).filter((e) => n.indexOf(e) !== -1), i = {
			...e,
			...t
		};
		Object.prototype.hasOwnProperty.call(i, "__proto__") && delete i.__proto__;
		for (let n of r) {
			if (n === "__proto__") continue;
			let r = Ma(e[n], t[n]);
			if (!r.valid) return {
				valid: !1,
				mergeErrorPath: [n, ...r.mergeErrorPath]
			};
			i[n] = r.data;
		}
		return {
			valid: !0,
			data: i
		};
	}
	if (Array.isArray(e) && Array.isArray(t)) {
		if (e.length !== t.length) return {
			valid: !1,
			mergeErrorPath: []
		};
		let n = [];
		for (let r = 0; r < e.length; r++) {
			let i = e[r], a = t[r], o = Ma(i, a);
			if (!o.valid) return {
				valid: !1,
				mergeErrorPath: [r, ...o.mergeErrorPath]
			};
			n.push(o.data);
		}
		return {
			valid: !0,
			data: n
		};
	}
	return {
		valid: !1,
		mergeErrorPath: []
	};
}
function Na(e, t, n) {
	let r = /* @__PURE__ */ new Map(), i, a = /* @__PURE__ */ new Map(), o = (e, t) => {
		let n;
		if (e.code === "unrecognized_keys" && !e.path?.length) i ??= e, n = e.keys;
		else if (e.code === "invalid_key" && e.origin === "record" && e.path?.length === 1) {
			let t = String(e.path[0]);
			a.has(t) || a.set(t, e), n = [t];
		} else return !1;
		for (let e of n) r.has(e) || r.set(e, {}), r.get(e)[t] = !0;
		return !0;
	};
	for (let n of t.issues) o(n, "l") || e.issues.push(n);
	for (let t of n.issues) o(t, "r") || e.issues.push(t);
	let s = [...r].filter(([, e]) => e.l && e.r).map(([e]) => e);
	if (s.length) {
		let t = i ? s.filter((e) => i.keys.includes(e)) : [];
		t.length && e.issues.push({
			...i,
			keys: t
		});
		for (let n of s) !t.includes(n) && a.has(n) && e.issues.push(a.get(n));
	}
	let c = Ma(t.value, n.value);
	if (!c.valid) {
		if (C(e)) return e;
		throw Error(`Unmergable intersection. Error path: ${JSON.stringify(c.mergeErrorPath)}`);
	}
	return e.value = c.data, e;
}
var Pa = /*@__PURE__*/ O("$ZodTuple", (e, t) => {
	N.init(e, t);
	let n = t.items, r = k.memoizer;
	r?.attach(e), e._zod.parse = (i, a) => {
		let o = i.value;
		if (!Array.isArray(o)) return i.issues.push({
			input: o,
			inst: e,
			expected: "tuple",
			code: "invalid_type"
		}), i;
		i.value = r ? r.alloc(e, i, [], a) : [];
		let s = [], c = Fa(n, "optin"), l = Fa(n, "optout");
		if (!t.rest) {
			if (o.length < c) return i.issues.push({
				code: "too_small",
				minimum: c,
				inclusive: !0,
				input: o,
				inst: e,
				origin: "array"
			}), i;
			o.length > n.length && i.issues.push({
				code: "too_big",
				maximum: n.length,
				inclusive: !0,
				input: o,
				inst: e,
				origin: "array"
			});
		}
		let u = Array(n.length), d = t.rest ? a?.abortEarly : void 0, f = !1;
		for (let e = 0; e < n.length; e++) {
			let t = n[e]._zod.run({
				value: o[e],
				issues: []
			}, a);
			t instanceof Promise ? s.push(t.then((t) => {
				u[e] = t;
			})) : (u[e] = t, d && !f && t.issues.length && (f = C(t)));
		}
		if (t.rest && !f) {
			let e = n.length - 1, r = o.slice(n.length), c = i.issues.length;
			for (let n of r) {
				if (d && i.issues.length !== c) {
					if (C(i, c)) break;
					c = i.issues.length;
				}
				e++;
				let r = t.rest._zod.run({
					value: n,
					issues: []
				}, a);
				r instanceof Promise ? s.push(r.then((t) => Ia(t, i, e))) : Ia(r, i, e);
			}
		}
		return s.length ? Promise.all(s).then(() => La(u, i, n, o, l)) : La(u, i, n, o, l);
	};
});
function Fa(e, t) {
	for (let n = e.length - 1; n >= 0; n--) if (!(t === "optin" ? e[n]._zod.optin !== void 0 : e[n]._zod.optout === "optional")) return n + 1;
	return 0;
}
function Ia(e, t, n) {
	e.issues.length && t.issues.push(...w(n, e.issues)), t.value[n] = e.value;
}
function La(e, t, n, r, i) {
	for (let a = 0; a < n.length; a++) {
		let o = e[a], s = a < r.length;
		if (!s && a >= i && n[a]._zod.optin === "optional") {
			t.value.length = a;
			break;
		}
		if (o.issues.length) {
			if (!s && a >= i) {
				t.value.length = a;
				break;
			}
			t.issues.push(...w(a, o.issues));
		}
		t.value[a] = o.value;
	}
	for (let e = t.value.length - 1; e >= r.length && n[e]._zod.optout === "optional" && t.value[e] === void 0; e--) t.value.length = e;
	return t;
}
var Ra = /*@__PURE__*/ O("$ZodRecord", (e, t) => {
	N.init(e, t);
	let n = k.memoizer;
	n?.attach(e), e._zod.parse = (r, i) => {
		let a = r.value;
		if (!_e(a)) return r.issues.push({
			expected: "record",
			code: "invalid_type",
			input: a,
			inst: e
		}), r;
		let o = [], s = t.keyType._zod.values;
		if (s && !t.partial) {
			r.value = n ? n.alloc(e, r, {}, i) : {};
			let c = /* @__PURE__ */ new Set();
			for (let n of s) if (typeof n == "string" || typeof n == "number" || typeof n == "symbol") {
				if (c.add(typeof n == "number" ? n.toString() : n), n === "__proto__") continue;
				let s = t.keyType._zod.run({
					value: n,
					issues: []
				}, i);
				if (s instanceof Promise) throw Error("Async schemas not supported in object keys currently");
				if (s.issues.length) {
					r.issues.push({
						code: "invalid_key",
						origin: "record",
						issues: s.issues.map((e) => T(e, i, A())),
						input: n,
						path: [n],
						inst: e
					});
					continue;
				}
				let l = s.value;
				if (l === "__proto__") continue;
				let u = t.valueType._zod.run({
					value: a[n],
					issues: []
				}, i);
				u instanceof Promise ? o.push(u.then((e) => {
					e.issues.length && r.issues.push(...w(n, e.issues)), r.value[l] = e.value;
				})) : (u.issues.length && r.issues.push(...w(n, u.issues)), r.value[l] = u.value);
			}
			let l;
			for (let e in a) if (!c.has(e)) {
				if (t.mode === "loose") {
					if (e === "__proto__") continue;
					r.value[e] = a[e];
				} else l ??= [], l.push(e);
			}
			l && l.length > 0 && r.issues.push({
				code: "unrecognized_keys",
				input: a,
				inst: e,
				keys: l,
				continue: !0
			});
		} else {
			r.value = n ? n.alloc(e, r, {}, i) : {};
			let c;
			for (let n of Reflect.ownKeys(a)) {
				if (n === "__proto__" || !Object.prototype.propertyIsEnumerable.call(a, n)) continue;
				let l = t.keyType._zod.run({
					value: n,
					issues: []
				}, i);
				if (l instanceof Promise) throw Error("Async schemas not supported in object keys currently");
				if (typeof n == "string" && ir.test(n) && l.issues.length) {
					let e = t.keyType._zod.run({
						value: Number(n),
						issues: []
					}, i);
					if (e instanceof Promise) throw Error("Async schemas not supported in object keys currently");
					e.issues.length === 0 && (l = e);
				}
				if (l.issues.length) {
					t.mode === "loose" ? r.value[n] = a[n] : s ? (c ??= [], c.push(n)) : r.issues.push({
						code: "invalid_key",
						origin: "record",
						issues: l.issues.map((e) => T(e, i, A())),
						input: n,
						path: [n],
						inst: e
					});
					continue;
				}
				let u = l.value;
				if (u === "__proto__") continue;
				let d = t.valueType._zod.run({
					value: a[n],
					issues: []
				}, i);
				d instanceof Promise ? o.push(d.then((e) => {
					e.issues.length && r.issues.push(...w(n, e.issues)), r.value[u] = e.value;
				})) : (d.issues.length && r.issues.push(...w(n, d.issues)), r.value[u] = d.value);
			}
			c && c.length > 0 && r.issues.push({
				code: "unrecognized_keys",
				input: a,
				inst: e,
				keys: c,
				continue: !0
			});
		}
		return o.length ? Promise.all(o).then(() => r) : r;
	};
}), za = /*@__PURE__*/ O("$ZodMap", (e, t) => {
	N.init(e, t);
	let n = k.memoizer;
	n?.attach(e), e._zod.parse = (r, i) => {
		let a = r.value;
		if (!(a instanceof Map)) return r.issues.push({
			expected: "map",
			code: "invalid_type",
			input: a,
			inst: e
		}), r;
		let o = [];
		r.value = n ? n.alloc(e, r, /* @__PURE__ */ new Map(), i) : /* @__PURE__ */ new Map();
		let s = i?.abortEarly, c = r.issues.length;
		for (let [n, l] of a) {
			if (s && r.issues.length !== c) {
				if (C(r, c)) break;
				c = r.issues.length;
			}
			let u = t.keyType._zod.run({
				value: n,
				issues: []
			}, i), d = t.valueType._zod.run({
				value: l,
				issues: []
			}, i);
			u instanceof Promise || d instanceof Promise ? o.push(Promise.all([u, d]).then(([t, o]) => {
				Ba(t, o, r, n, a, e, i);
			})) : Ba(u, d, r, n, a, e, i);
		}
		return o.length ? Promise.all(o).then(() => r) : r;
	}, fa(e, ma);
});
function Ba(e, t, n, r, i, a, o) {
	e.issues.length && (xe.has(typeof r) ? n.issues.push(...w(r, e.issues)) : n.issues.push({
		code: "invalid_key",
		origin: "map",
		input: i,
		inst: a,
		issues: e.issues.map((e) => T(e, o, A()))
	})), t.issues.length && (xe.has(typeof r) ? n.issues.push(...w(r, t.issues)) : n.issues.push({
		origin: "map",
		code: "invalid_element",
		input: i,
		inst: a,
		key: r,
		issues: t.issues.map((e) => T(e, o, A()))
	})), n.value.set(e.value, t.value);
}
var Va = /*@__PURE__*/ O("$ZodSet", (e, t) => {
	N.init(e, t);
	let n = k.memoizer;
	n?.attach(e), e._zod.parse = (r, i) => {
		let a = r.value;
		if (!(a instanceof Set)) return r.issues.push({
			input: a,
			inst: e,
			expected: "set",
			code: "invalid_type"
		}), r;
		let o = [];
		r.value = n ? n.alloc(e, r, /* @__PURE__ */ new Set(), i) : /* @__PURE__ */ new Set();
		let s = i?.abortEarly, c = r.issues.length;
		for (let e of a) {
			if (s && r.issues.length !== c) {
				if (C(r, c)) break;
				c = r.issues.length;
			}
			let n = t.valueType._zod.run({
				value: e,
				issues: []
			}, i);
			n instanceof Promise ? o.push(n.then((e) => Ha(e, r))) : Ha(n, r);
		}
		return o.length ? Promise.all(o).then(() => r) : r;
	}, fa(e, ha);
});
function Ha(e, t) {
	e.issues.length && t.issues.push(...e.issues), t.value.add(e.value);
}
var Ua = /*@__PURE__*/ O("$ZodEnum", (e, t) => {
	N.init(e, t);
	let n = l(t.entries), r = new Set(n);
	e._zod.values = r, D(e, "pattern", (e) => {
		let t = l(e.def.entries).filter((e) => xe.has(typeof e));
		return RegExp(t.length ? `^(${t.map((e) => Ce(e.toString())).join("|")})$` : "^[^\\s\\S]$");
	}), e._zod.parse = (t, i) => {
		let a = t.value;
		return r.has(a) || t.issues.push({
			code: "invalid_value",
			values: n,
			input: a,
			inst: e
		}), t;
	};
}), Wa = /*@__PURE__*/ O("$ZodLiteral", (e, t) => {
	N.init(e, t);
	let n = new Set(t.values);
	e._zod.values = n, D(e, "pattern", (e) => {
		let t = e.def.values;
		return RegExp(t.length ? `^(${t.map((e) => typeof e == "string" ? Ce(e) : e ? Ce(e.toString()) : String(e)).join("|")})$` : "^[^\\s\\S]$");
	}), e._zod.parse = (r, i) => {
		let a = r.value;
		return n.has(a) || r.issues.push({
			code: "invalid_value",
			values: t.values,
			input: a,
			inst: e
		}), r;
	};
}), Ga = /*@__PURE__*/ O("$ZodFile", (e, t) => {
	N.init(e, t), e._zod.parse = (t, n) => {
		let r = t.value;
		return r instanceof File || t.issues.push({
			expected: "file",
			code: "invalid_type",
			input: r,
			inst: e
		}), t;
	};
}), Ka = /*@__PURE__*/ O("$ZodTransform", (e, t) => {
	N.init(e, t), e._zod.optin = "optional", k.memoizer?.guard(e), e._zod.parse = (n, r) => {
		if (r.direction === "backward") throw new yt(e.constructor.name);
		let i = t.transform(n.value, n);
		if (r.async) return (i instanceof Promise ? i : Promise.resolve(i)).then((e) => (n.value = e, n));
		if (i instanceof Promise) throw new vt();
		return n.value = i, n;
	};
});
function qa(e, t) {
	return e.value = t.issues.length ? void 0 : t.value, e;
}
var Ja = /*@__PURE__*/ O("$ZodOptional", (e, t) => {
	N.init(e, t), D(e, "optin", (e) => e.def.innerType._zod.optin === "defaulted" ? "defaulted" : "optional"), e._zod.optout = "optional", D(e, "values", (e) => {
		let t = e.def.innerType._zod.values;
		return t ? /* @__PURE__ */ new Set([...t, void 0]) : void 0;
	}), D(e, "pattern", (e) => {
		let t = e.def.innerType._zod.pattern;
		return t ? RegExp(`^(${h(t.source)})?$`) : void 0;
	}), e._zod.parse = (e, n) => {
		if (e.value === void 0) {
			if (t.innerType._zod.optin !== "defaulted") return e;
			let r = t.innerType._zod.run({
				value: e.value,
				issues: []
			}, n);
			return r instanceof Promise ? r.then((t) => qa(e, t)) : qa(e, r);
		}
		return t.innerType._zod.run(e, n);
	};
}), Ya = /*@__PURE__*/ O("$ZodExactOptional", (e, t) => {
	Ja.init(e, t), D(e, "values", (e) => e.def.innerType._zod.values), D(e, "pattern", (e) => e.def.innerType._zod.pattern), e._zod.parse = (e, n) => t.innerType._zod.run(e, n);
}), Xa = /*@__PURE__*/ O("$ZodNullable", (e, t) => {
	N.init(e, t), D(e, "optin", (e) => e.def.innerType._zod.optin), D(e, "optout", (e) => e.def.innerType._zod.optout), D(e, "pattern", (e) => {
		let t = e.def.innerType._zod.pattern;
		return t ? RegExp(`^(${h(t.source)}|null)$`) : void 0;
	}), D(e, "values", (e) => e.def.innerType._zod.values ? /* @__PURE__ */ new Set([...e.def.innerType._zod.values, null]) : void 0), e._zod.parse = (e, n) => e.value === null ? e : t.innerType._zod.run(e, n);
}), Za = /*@__PURE__*/ O("$ZodDefault", (e, t) => {
	N.init(e, t), e._zod.optin = "defaulted", D(e, "values", (e) => e.def.innerType._zod.values), e._zod.parse = (e, n) => {
		if (n.direction === "backward") return t.innerType._zod.run(e, n);
		if (e.value === void 0) return e.value = t.defaultValue, e;
		let r = t.innerType._zod.run(e, n);
		return r instanceof Promise ? r.then((e) => Qa(e, t)) : Qa(r, t);
	};
});
function Qa(e, t) {
	return e.value === void 0 && (e.value = t.defaultValue), e;
}
var $a = /*@__PURE__*/ O("$ZodPrefault", (e, t) => {
	N.init(e, t), e._zod.optin = "defaulted", D(e, "values", (e) => e.def.innerType._zod.values), e._zod.parse = (e, n) => (n.direction === "backward" || e.value === void 0 && (e.value = t.defaultValue), t.innerType._zod.run(e, n));
}), eo = /*@__PURE__*/ O("$ZodNonOptional", (e, t) => {
	N.init(e, t), D(e, "values", (e) => {
		let t = e.def.innerType._zod.values;
		return t ? new Set([...t].filter((e) => e !== void 0)) : void 0;
	}), e._zod.parse = (n, r) => {
		let i = t.innerType._zod.run(n, r);
		return i instanceof Promise ? i.then((t) => to(t, e)) : to(i, e);
	};
});
function to(e, t) {
	return !e.issues.length && e.value === void 0 && e.issues.push({
		code: "invalid_type",
		expected: "nonoptional",
		input: e.value,
		inst: t
	}), e;
}
var no = /*@__PURE__*/ O("$ZodSuccess", (e, t) => {
	N.init(e, t), e._zod.parse = (e, n) => {
		if (n.direction === "backward") throw new yt("ZodSuccess");
		let r = t.innerType._zod.run(e, n);
		return r instanceof Promise ? r.then((t) => (e.value = t.issues.length === 0, e)) : (e.value = r.issues.length === 0, e);
	};
});
function ro(e, t, n, r) {
	return t.issues.length ? (e.value = n.catchValue({
		...t,
		value: e.value,
		error: { issues: t.issues.map((e) => T(e, r, A())) },
		input: e.value
	}), e) : (e.value = t.value, t.memo && (e.memo = !0), e);
}
var io = /*@__PURE__*/ O("$ZodCatch", (e, t) => {
	N.init(e, t), D(e, "optin", (e) => e.def.innerType._zod.optin === "defaulted" ? "defaulted" : "optional"), D(e, "optout", (e) => e.def.innerType._zod.optout), D(e, "values", (e) => e.def.innerType._zod.values), e._zod.parse = (e, n) => {
		if (n.direction === "backward") return t.innerType._zod.run(e, n);
		let r = t.innerType._zod.run({
			value: e.value,
			issues: []
		}, n);
		return r instanceof Promise ? r.then((r) => ro(e, r, t, n)) : ro(e, r, t, n);
	};
}), ao = /*@__PURE__*/ O("$ZodNaN", (e, t) => {
	N.init(e, t), e._zod.parse = (t, n) => (typeof t.value == "number" && Number.isNaN(t.value) || t.issues.push({
		input: t.value,
		inst: e,
		expected: "nan",
		code: "invalid_type"
	}), t);
}), oo = /*@__PURE__*/ O("$ZodPipe", (e, t) => {
	N.init(e, t), D(e, "values", (e) => e.def.in._zod.values), D(e, "optin", (e) => e.def.in._zod.optin), D(e, "optout", (e) => e.def.out._zod.optout), D(e, "propValues", (e) => e.def.in._zod.propValues), e._zod.parse = (e, n) => {
		if (n.direction === "backward") {
			let r = t.out._zod.run(e, n);
			return r instanceof Promise ? r.then((e) => so(e, t.in, n)) : so(r, t.in, n);
		}
		let r = t.in._zod.run(e, n);
		return r instanceof Promise ? r.then((e) => so(e, t.out, n)) : so(r, t.out, n);
	};
});
function so(e, t, n) {
	return e.issues.some((e) => e.code !== "unrecognized_keys") ? (e.aborted = !0, e) : t._zod.run({
		value: e.value,
		issues: e.issues
	}, n);
}
var co = /*@__PURE__*/ O("$ZodCodec", (e, t) => {
	N.init(e, t), D(e, "values", (e) => e.def.in._zod.values), D(e, "optin", (e) => e.def.in._zod.optin), D(e, "optout", (e) => e.def.out._zod.optout), D(e, "propValues", (e) => e.def.in._zod.propValues), e._zod.parse = (e, n) => {
		if ((n.direction || "forward") === "forward") {
			let r = t.in._zod.run(e, n);
			return r instanceof Promise ? r.then((e) => lo(e, t, n)) : lo(r, t, n);
		}
		{
			let r = t.out._zod.run(e, n);
			return r instanceof Promise ? r.then((e) => lo(e, t, n)) : lo(r, t, n);
		}
	};
});
function lo(e, t, n) {
	if (e.issues.length) return e.aborted = !0, e;
	if ((n.direction || "forward") === "forward") {
		let r = t.transform(e.value, e);
		return r instanceof Promise ? r.then((r) => uo(e, r, t.out, n)) : uo(e, r, t.out, n);
	}
	{
		let r = t.reverseTransform(e.value, e);
		return r instanceof Promise ? r.then((r) => uo(e, r, t.in, n)) : uo(e, r, t.in, n);
	}
}
function uo(e, t, n, r) {
	return e.issues.length ? (e.aborted = !0, e) : n._zod.run({
		value: t,
		issues: e.issues
	}, r);
}
var fo = /*@__PURE__*/ O("$ZodPreprocess", (e, t) => {
	oo.init(e, t);
}), po = /*@__PURE__*/ O("$ZodReadonly", (e, t) => {
	N.init(e, t), D(e, "propValues", (e) => e.def.innerType._zod.propValues), D(e, "values", (e) => e.def.innerType._zod.values), D(e, "optin", (e) => e.def.innerType?._zod?.optin), D(e, "optout", (e) => e.def.innerType?._zod?.optout), e._zod.parse = (e, n) => {
		if (n.direction === "backward") return t.innerType._zod.run(e, n);
		let r = t.innerType._zod.run(e, n);
		return r instanceof Promise ? r.then(mo) : mo(r);
	};
});
function mo(e) {
	return e.memo || (e.value = Object.freeze(e.value)), e;
}
function ho(e) {
	let t = e._zod.def, n = t.pattern, r = !!t.format?.includes("int"), i, a;
	for (let e of t.checks ?? []) {
		let t = e._zod.def;
		t.pattern && (n = t.pattern), r ||= !!t.format?.includes("int");
		let o = t.minimum ?? t.length, s = t.maximum ?? t.length;
		o !== void 0 && (i === void 0 || o > i) && (i = o), s !== void 0 && (a === void 0 || s < a) && (a = s);
	}
	if (n) return n.source;
	if (i !== void 0 && a !== void 0 && i > a) return "(?!)";
	if (i !== void 0 || a !== void 0) return tr({
		minimum: i,
		maximum: a
	}).source;
	let o = e._zod.pattern;
	return (r && o === ir ? rr : o)?.source;
}
function go(e) {
	let t = e._zod.def, n = e._zod.pattern?.source, r = t.innerType ?? e._zod.innerType;
	if (r) {
		let e = r._zod.pattern?.source, t = go(r);
		return n && e && t && t !== e ? n.replace(h(e), () => h(t)) : n;
	}
	if (t.options) {
		let e = t.options.map(go);
		if (e.every(Boolean)) return `^(${e.map((e) => h(e)).join("|")})$`;
	}
	return ho(e);
}
var _o = /*@__PURE__*/ O("$ZodTemplateLiteral", (e, t) => {
	N.init(e, t);
	let n = [];
	for (let e of t.parts) if (typeof e == "object" && e) {
		let t = go(e);
		if (!t) throw Error(`Invalid template literal part, no pattern found: ${[...e._zod.traits].shift()}`);
		n.push(h(t));
	} else if (e === null || Se.has(typeof e)) n.push(Ce(`${e}`));
	else throw Error(`Invalid template literal part: ${e}`);
	e._zod.pattern = RegExp(`^${n.join("")}$`), e._zod.parse = (n, r) => typeof n.value == "string" ? (e._zod.pattern.lastIndex = 0, e._zod.pattern.test(n.value) || n.issues.push({
		input: n.value,
		inst: e,
		code: "invalid_format",
		format: t.format ?? "template_literal",
		pattern: e._zod.pattern.source
	}), n) : (n.issues.push({
		input: n.value,
		inst: e,
		expected: "string",
		code: "invalid_type"
	}), n);
}), vo = /*@__PURE__*/ O("$ZodFunction", (e, t) => (N.init(e, t), Object.defineProperty(e, "_def", { value: t }), e._zod.def = t, e.implement = (t) => {
	if (typeof t != "function") throw Error("implement() must be called with a function");
	return Object.defineProperty(function(...n) {
		let r = e._def.input ? Ft(e._def.input, n) : n, i = Reflect.apply(t, this, r);
		return e._def.output ? Ft(e._def.output, i) : i;
	}, "_zod", {
		value: e._zod,
		enumerable: !1
	});
}, e.implementAsync = (t) => {
	if (typeof t != "function") throw Error("implementAsync() must be called with a function");
	return Object.defineProperty(async function(...n) {
		let r = e._def.input ? await Lt(e._def.input, n) : n, i = await Reflect.apply(t, this, r);
		return e._def.output ? await Lt(e._def.output, i) : i;
	}, "_zod", {
		value: e._zod,
		enumerable: !1
	});
}, e._zod.parse = (t, n) => typeof t.value == "function" ? (t.value = e._def.output && e._def.output._zod.def.type === "promise" ? e.implementAsync(t.value) : e.implement(t.value), t) : (t.issues.push({
	code: "invalid_type",
	expected: "function",
	input: t.value,
	inst: e
}), t), e.input = (...t) => {
	let n = e.constructor;
	return Array.isArray(t[0]) ? new n({
		type: "function",
		input: new Pa({
			type: "tuple",
			items: t[0],
			rest: t[1]
		}),
		output: e._def.output
	}) : new n({
		type: "function",
		input: t[0],
		output: e._def.output
	});
}, e.output = (t) => {
	let n = e.constructor;
	return new n({
		type: "function",
		input: e._def.input,
		output: t
	});
}, e)), yo = /*@__PURE__*/ O("$ZodPromise", (e, t) => {
	N.init(e, t), e._zod.parse = (e, n) => Promise.resolve(e.value).then((e) => t.innerType._zod.run({
		value: e,
		issues: []
	}, n));
}), bo = /*@__PURE__*/ O("$ZodLazy", (e, t) => {
	N.init(e, t), te(e._zod, "innerType", () => {
		let e = t;
		return e._cachedInner ||= t.getter(), e._cachedInner;
	}), D(e, "pattern", (e) => e.innerType?._zod?.pattern), D(e, "propValues", (e) => e.innerType?._zod?.propValues), D(e, "optin", (e) => e.innerType?._zod?.optin ?? void 0), D(e, "optout", (e) => e.innerType?._zod?.optout ?? void 0), e._zod.parse = (t, n) => e._zod.innerType._zod.run(t, n);
}), xo = /*@__PURE__*/ O("$ZodCustom", (e, t) => {
	M.init(e, t), N.init(e, t), e._zod.parse = (e, t) => e, e._zod.check = (n) => {
		let r = n.value, i = t.fn(r);
		if (i instanceof Promise) return i.then((t) => So(t, n, r, e));
		So(i, n, r, e);
	};
});
function So(e, t, n, r) {
	if (!e) {
		let e = {
			code: "custom",
			input: n,
			inst: r,
			path: [...r._zod.def.path ?? []],
			continue: !r._zod.def.abort
		};
		r._zod.def.params && (e.params = r._zod.def.params), t.issues.push(Ge(e));
	}
}
// node_modules/zod/v4/core/memoizer.js
var Co = class extends Error {
	constructor() {
		super("Cannot parse a reference cycle that closes through a transform"), this.name = "ZodCyclicError";
	}
}, wo = "~memo", To = [];
function Eo(e) {
	return typeof e == "object" && !!e;
}
function Do(e) {
	return e.map((e) => e.path ? {
		...e,
		path: e.path.slice()
	} : { ...e });
}
var Oo = /*@__PURE__*/ new WeakMap(), ko = 0, Ao = 1, jo = 2;
function Mo(e, t, n) {
	let r = Oo.get(e);
	if (r !== void 0) return r ? jo : ko;
	if (t.has(e)) return jo;
	t.add(e);
	let i = ko, a = (e) => {
		if (i !== jo && e?._zod) {
			let r = Mo(e, t, n);
			r > i && (i = r);
		}
	}, o = (e, r) => {
		let i = ko;
		for (let a of Reflect.ownKeys(e)) {
			let o = Object.getOwnPropertyDescriptor(e, a);
			if (r && !o.enumerable) continue;
			let s = o.get ? Ao : o.value?._zod ? Mo(o.value, t, n) : ko;
			s > i && (i = s);
		}
		return i;
	}, s = (e) => {
		e > i && (i = e);
	}, c = e._zod.def;
	switch (c.type) {
		case "object": {
			let e = re(c);
			s(e ? o(e, !0) : Ao), a(c.catchall);
			break;
		}
		case "array":
			a(c.element);
			break;
		case "tuple":
			for (let e of c.items) a(e);
			a(c.rest);
			break;
		case "record":
		case "map":
			a(c.keyType), a(c.valueType);
			break;
		case "set":
			a(c.valueType);
			break;
		case "union":
			for (let e of c.options) a(e);
			break;
		case "intersection":
			a(c.left), a(c.right);
			break;
		case "optional":
		case "nullable":
		case "default":
		case "prefault":
		case "catch":
		case "readonly":
		case "nonoptional":
		case "promise":
		case "success":
			a(c.innerType);
			break;
		case "pipe":
			a(c.in), a(c.out);
			break;
		case "function":
			a(c.input), a(c.output);
			break;
		case "lazy": {
			let r = c._cachedInner ?? (n ? e._zod.innerType : void 0);
			s(r ? Mo(r, t, !1) : Ao);
			break;
		}
		case "template_literal":
		case "string":
		case "number":
		case "int":
		case "boolean":
		case "bigint":
		case "symbol":
		case "undefined":
		case "null":
		case "void":
		case "never":
		case "any":
		case "unknown":
		case "date":
		case "nan":
		case "enum":
		case "literal":
		case "file":
		case "transform":
		case "custom": break;
		default: for (let e in c) {
			let t = Object.getOwnPropertyDescriptor(c, e);
			if (!t || t.get) continue;
			let n = t.value;
			if (n && typeof n == "object") {
				if (n._zod) a(n);
				else if (Array.isArray(n)) for (let e of n) a(e);
			}
		}
	}
	return t.delete(e), No(e, i);
}
function No(e, t) {
	return t !== Ao && Oo.set(e, t === jo), t;
}
function Po(e) {
	return Mo(e, /* @__PURE__ */ new Set(), !0) !== ko;
}
function Fo(e, t) {
	let n = e.buckets.get(t);
	return n || (n = /* @__PURE__ */ new WeakMap(), e.buckets.set(t, n)), n;
}
var Io, Lo = [], Ro = {
	alloc(e, t, n) {
		let r = Io;
		if (!r) return n;
		Io = void 0;
		let i = {
			value: n,
			issues: null
		};
		return r.set(t.value, i), Lo.push(i), n;
	},
	guard(e) {
		var t;
		(t = e._zod).deferred ?? (t.deferred = []), e._zod.deferred.push(() => {
			let t = e._zod.parse, n = (e, n) => {
				if (n.direction !== "backward" && Bo(n, e.value)) throw new Co();
				return t(e, n);
			};
			e._zod.parse = n, e._zod.run === t && (e._zod.run = n);
		});
	},
	attach(e) {
		var t;
		let n, r = !1, i, a;
		(t = e._zod).deferred ?? (t.deferred = []), e._zod.deferred.push(() => {
			let t = e._zod.parse, o = (s, c) => {
				if (n === void 0) {
					let i = Mo(e, /* @__PURE__ */ new Set(), !1);
					if (i === ko) return e._zod.parse = t, e._zod.run === o && (e._zod.run = t), t(s, c);
					i === jo || r ? n = !0 : r = !0;
				}
				let l = s.value;
				if (!Eo(l)) return t(s, c);
				let u = c[wo];
				u || (u = {
					buckets: /* @__PURE__ */ new WeakMap(),
					backEdges: void 0
				}, c[wo] = u);
				let d;
				i === c ? d = a : (d = Fo(u, e), i = c, a = d);
				let f = d.get(l);
				if (f) return s.value = f.value, f.issues ? f.issues.length && s.issues.push(...Do(f.issues)) : (s.memo = !0, u.backEdges ?? (u.backEdges = /* @__PURE__ */ new WeakSet()), u.backEdges.add(f.value)), s;
				Io = d;
				let p = Lo.length, m = t(s, c);
				Io = void 0;
				let h = Lo.length > p ? Lo.pop() : void 0;
				return m instanceof Promise ? m.then((e) => (h && (h.issues = e.issues.length ? Do(e.issues) : To), e)) : (h && (h.issues = m.issues.length ? Do(m.issues) : To), m);
			};
			e._zod.parse = o, e._zod.run === t && (e._zod.run = o);
		});
	}
};
function zo() {
	return Ro;
}
function Bo(e, t) {
	let n = e[wo]?.backEdges;
	return n !== void 0 && Eo(t) && n.has(t);
}
// node_modules/zod/v4/locales/ar.js
var Vo = () => {
	let e = {
		string: {
			unit: "حرف",
			verb: "أن يحوي"
		},
		file: {
			unit: "بايت",
			verb: "أن يحوي"
		},
		array: {
			unit: "عنصر",
			verb: "أن يحوي"
		},
		set: {
			unit: "عنصر",
			verb: "أن يحوي"
		},
		map: {
			unit: "عنصر",
			verb: "أن يحوي"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "مدخل",
		email: "بريد إلكتروني",
		url: "رابط",
		emoji: "إيموجي",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "تاريخ ووقت بمعيار ISO",
		date: "تاريخ بمعيار ISO",
		time: "وقت بمعيار ISO",
		duration: "مدة بمعيار ISO",
		ipv4: "عنوان IPv4",
		ipv6: "عنوان IPv6",
		mac: "عنوان MAC",
		cidrv4: "مدى عناوين بصيغة IPv4",
		cidrv6: "مدى عناوين بصيغة IPv6",
		base64: "نَص بترميز base64-encoded",
		base64url: "نَص بترميز base64url-encoded",
		json_string: "نَص على هيئة JSON",
		e164: "رقم هاتف بمعيار E.164",
		credit_card: "رقم بطاقة الائتمان",
		currency_code: "رمز العملة",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "مدخل"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `مدخلات غير مقبولة: يفترض إدخال instanceof ${e.expected}، ولكن تم إدخال ${i}` : `مدخلات غير مقبولة: يفترض إدخال ${t}، ولكن تم إدخال ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `مدخلات غير مقبولة: يفترض إدخال ${S(e.values[0])}` : `اختيار غير مقبول: يتوقع انتقاء أحد هذه الخيارات: ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? ` أكبر من اللازم: يفترض أن تكون ${e.origin ?? "القيمة"} ${n} ${e.maximum.toString()} ${r.unit ?? "عنصر"}` : `أكبر من اللازم: يفترض أن تكون ${e.origin ?? "القيمة"} ${n} ${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `أصغر من اللازم: يفترض لـ ${e.origin} أن يكون ${n} ${e.minimum.toString()} ${r.unit}` : `أصغر من اللازم: يفترض لـ ${e.origin} أن يكون ${n} ${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `نَص غير مقبول: يجب أن يبدأ بـ "${e.prefix}"` : t.format === "ends_with" ? `نَص غير مقبول: يجب أن ينتهي بـ "${t.suffix}"` : t.format === "includes" ? `نَص غير مقبول: يجب أن يتضمَّن "${t.includes}"` : t.format === "regex" ? `نَص غير مقبول: يجب أن يطابق النمط ${t.pattern}` : `${n[t.format] ?? e.format} غير مقبول`;
			}
			case "not_multiple_of": return `رقم غير مقبول: يجب أن يكون من مضاعفات ${e.divisor}`;
			case "unrecognized_keys": return `معرف${e.keys.length > 1 ? "ات" : ""} غريب${e.keys.length > 1 ? "ة" : ""}: ${u(e.keys, "، ")}`;
			case "invalid_key": return `معرف غير مقبول في ${e.origin}`;
			case "invalid_union": return "مدخل غير مقبول";
			case "invalid_element": return `مدخل غير مقبول في ${e.origin}`;
			default: return "مدخل غير مقبول";
		}
	};
};
function Ho() {
	return { localeError: Vo() };
}
// node_modules/zod/v4/locales/az.js
var Uo = () => {
	let e = {
		string: {
			unit: "simvol",
			verb: "olmalıdır"
		},
		file: {
			unit: "bayt",
			verb: "olmalıdır"
		},
		array: {
			unit: "element",
			verb: "olmalıdır"
		},
		set: {
			unit: "element",
			verb: "olmalıdır"
		},
		map: {
			unit: "element",
			verb: "olmalıdır"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "input",
		email: "email address",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO datetime",
		date: "ISO date",
		time: "ISO time",
		duration: "ISO duration",
		ipv4: "IPv4 address",
		ipv6: "IPv6 address",
		mac: "MAC address",
		cidrv4: "IPv4 range",
		cidrv6: "IPv6 range",
		base64: "base64-encoded string",
		base64url: "base64url-encoded string",
		json_string: "JSON string",
		e164: "E.164 number",
		credit_card: "kredit kartı nömrəsi",
		currency_code: "valyuta kodu",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "input"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Yanlış dəyər: gözlənilən instanceof ${e.expected}, daxil olan ${i}` : `Yanlış dəyər: gözlənilən ${t}, daxil olan ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Yanlış dəyər: gözlənilən ${S(e.values[0])}` : `Yanlış seçim: aşağıdakılardan biri olmalıdır: ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Çox böyük: gözlənilən ${e.origin ?? "dəyər"} ${n}${e.maximum.toString()} ${r.unit ?? "element"}` : `Çox böyük: gözlənilən ${e.origin ?? "dəyər"} ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Çox kiçik: gözlənilən ${e.origin} ${n}${e.minimum.toString()} ${r.unit}` : `Çox kiçik: gözlənilən ${e.origin} ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Yanlış mətn: "${t.prefix}" ilə başlamalıdır` : t.format === "ends_with" ? `Yanlış mətn: "${t.suffix}" ilə bitməlidir` : t.format === "includes" ? `Yanlış mətn: "${t.includes}" daxil olmalıdır` : t.format === "regex" ? `Yanlış mətn: ${t.pattern} şablonuna uyğun olmalıdır` : `Yanlış ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Yanlış ədəd: ${e.divisor} ilə bölünə bilən olmalıdır`;
			case "unrecognized_keys": return `Tanınmayan açar${e.keys.length > 1 ? "lar" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `${e.origin} daxilində yanlış açar`;
			case "invalid_union": return "Yanlış dəyər";
			case "invalid_element": return `${e.origin} daxilində yanlış dəyər`;
			default: return "Yanlış dəyər";
		}
	};
};
function Wo() {
	return { localeError: Uo() };
}
// node_modules/zod/v4/locales/be.js
function Go(e, t, n, r) {
	let i = Math.abs(e), a = i % 10, o = i % 100;
	return o >= 11 && o <= 19 ? r : a === 1 ? t : a >= 2 && a <= 4 ? n : r;
}
var Ko = () => {
	let e = {
		string: {
			unit: {
				one: "сімвал",
				few: "сімвалы",
				many: "сімвалаў"
			},
			verb: "мець"
		},
		array: {
			unit: {
				one: "элемент",
				few: "элементы",
				many: "элементаў"
			},
			verb: "мець"
		},
		set: {
			unit: {
				one: "элемент",
				few: "элементы",
				many: "элементаў"
			},
			verb: "мець"
		},
		map: {
			unit: {
				one: "элемент",
				few: "элементы",
				many: "элементаў"
			},
			verb: "мець"
		},
		file: {
			unit: {
				one: "байт",
				few: "байты",
				many: "байтаў"
			},
			verb: "мець"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "увод",
		email: "email адрас",
		url: "URL",
		emoji: "эмодзі",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO дата і час",
		date: "ISO дата",
		time: "ISO час",
		duration: "ISO працягласць",
		ipv4: "IPv4 адрас",
		ipv6: "IPv6 адрас",
		mac: "MAC адрас",
		cidrv4: "IPv4 дыяпазон",
		cidrv6: "IPv6 дыяпазон",
		base64: "радок у фармаце base64",
		base64url: "радок у фармаце base64url",
		json_string: "JSON радок",
		e164: "нумар E.164",
		credit_card: "нумар крэдытнай карты",
		currency_code: "код валюты",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "увод"
	}, r = {
		nan: "NaN",
		number: "лік",
		array: "масіў"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Няправільны ўвод: чакаўся instanceof ${e.expected}, атрымана ${i}` : `Няправільны ўвод: чакаўся ${t}, атрымана ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Няправільны ўвод: чакалася ${S(e.values[0])}` : `Няправільны варыянт: чакаўся адзін з ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				if (r) {
					let t = Go(Number(e.maximum), r.unit.one, r.unit.few, r.unit.many);
					return `Занадта вялікі: чакалася, што ${e.origin ?? "значэнне"} павінна ${r.verb} ${n}${e.maximum.toString()} ${t}`;
				}
				return `Занадта вялікі: чакалася, што ${e.origin ?? "значэнне"} павінна быць ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				if (r) {
					let t = Go(Number(e.minimum), r.unit.one, r.unit.few, r.unit.many);
					return `Занадта малы: чакалася, што ${e.origin} павінна ${r.verb} ${n}${e.minimum.toString()} ${t}`;
				}
				return `Занадта малы: чакалася, што ${e.origin} павінна быць ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Няправільны радок: павінен пачынацца з "${t.prefix}"` : t.format === "ends_with" ? `Няправільны радок: павінен заканчвацца на "${t.suffix}"` : t.format === "includes" ? `Няправільны радок: павінен змяшчаць "${t.includes}"` : t.format === "regex" ? `Няправільны радок: павінен адпавядаць шаблону ${t.pattern}` : `Няправільны ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Няправільны лік: павінен быць кратным ${e.divisor}`;
			case "unrecognized_keys": return `Нераспазнаны ${e.keys.length > 1 ? "ключы" : "ключ"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Няправільны ключ у ${e.origin}`;
			case "invalid_union": return "Няправільны ўвод";
			case "invalid_element": return `Няправільнае значэнне ў ${e.origin}`;
			default: return "Няправільны ўвод";
		}
	};
};
function qo() {
	return { localeError: Ko() };
}
// node_modules/zod/v4/locales/bg.js
var Jo = () => {
	let e = {
		string: {
			unit: "символа",
			verb: "да съдържа"
		},
		file: {
			unit: "байта",
			verb: "да съдържа"
		},
		array: {
			unit: "елемента",
			verb: "да съдържа"
		},
		set: {
			unit: "елемента",
			verb: "да съдържа"
		},
		map: {
			unit: "елемента",
			verb: "да съдържа"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "вход",
		email: "имейл адрес",
		url: "URL",
		emoji: "емоджи",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO време",
		date: "ISO дата",
		time: "ISO време",
		duration: "ISO продължителност",
		ipv4: "IPv4 адрес",
		ipv6: "IPv6 адрес",
		mac: "MAC адрес",
		cidrv4: "IPv4 диапазон",
		cidrv6: "IPv6 диапазон",
		base64: "base64-кодиран низ",
		base64url: "base64url-кодиран низ",
		json_string: "JSON низ",
		e164: "E.164 номер",
		credit_card: "номер на кредитна карта",
		currency_code: "код на валута",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "вход"
	}, r = {
		nan: "NaN",
		number: "число",
		array: "масив"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Невалиден вход: очакван instanceof ${e.expected}, получен ${i}` : `Невалиден вход: очакван ${t}, получен ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Невалиден вход: очакван ${S(e.values[0])}` : `Невалидна опция: очаквано едно от ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Твърде голямо: очаква се ${e.origin ?? "стойност"} да съдържа ${n}${e.maximum.toString()} ${r.unit ?? "елемента"}` : `Твърде голямо: очаква се ${e.origin ?? "стойност"} да бъде ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Твърде малко: очаква се ${e.origin} да съдържа ${n}${e.minimum.toString()} ${r.unit}` : `Твърде малко: очаква се ${e.origin} да бъде ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				if (t.format === "starts_with") return `Невалиден низ: трябва да започва с "${t.prefix}"`;
				if (t.format === "ends_with") return `Невалиден низ: трябва да завършва с "${t.suffix}"`;
				if (t.format === "includes") return `Невалиден низ: трябва да включва "${t.includes}"`;
				if (t.format === "regex") return `Невалиден низ: трябва да съвпада с ${t.pattern}`;
				let r = "Невалиден";
				return t.format === "emoji" && (r = "Невалидно"), t.format === "datetime" && (r = "Невалидно"), t.format === "date" && (r = "Невалидна"), t.format === "time" && (r = "Невалидно"), t.format === "duration" && (r = "Невалидна"), `${r} ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Невалидно число: трябва да бъде кратно на ${e.divisor}`;
			case "unrecognized_keys": return `Неразпознат${e.keys.length > 1 ? "и" : ""} ключ${e.keys.length > 1 ? "ове" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Невалиден ключ в ${e.origin}`;
			case "invalid_union": return "Невалиден вход";
			case "invalid_element": return `Невалидна стойност в ${e.origin}`;
			default: return "Невалиден вход";
		}
	};
};
function Yo() {
	return { localeError: Jo() };
}
// node_modules/zod/v4/locales/bn.js
var Xo = () => {
	let e = {
		string: {
			unit: "অক্ষর",
			verb: "থাকতে হবে"
		},
		file: {
			unit: "বাইট",
			verb: "থাকতে হবে"
		},
		array: {
			unit: "আইটেম",
			verb: "থাকতে হবে"
		},
		set: {
			unit: "আইটেম",
			verb: "থাকতে হবে"
		},
		map: {
			unit: "এন্ট্রি",
			verb: "থাকতে হবে"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "ইনপুট",
		email: "ইমেইল ঠিকানা",
		url: "URL",
		emoji: "ইমোজি",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO তারিখ ও সময়",
		date: "ISO তারিখ",
		time: "ISO সময়",
		duration: "ISO সময়কাল",
		ipv4: "IPv4 ঠিকানা",
		ipv6: "IPv6 ঠিকানা",
		mac: "MAC ঠিকানা",
		cidrv4: "IPv4 রেঞ্জ",
		cidrv6: "IPv6 রেঞ্জ",
		base64: "base64-এনকোডেড স্ট্রিং",
		base64url: "base64url-এনকোডেড স্ট্রিং",
		json_string: "JSON স্ট্রিং",
		e164: "E.164 নম্বর",
		credit_card: "ক্রেডিট কার্ড নম্বর",
		currency_code: "মুদ্রা কোড",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "ইনপুট"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input);
				return `অবৈধ ইনপুট: প্রত্যাশিত ${t}, প্রাপ্ত ${r[n] ?? n}`;
			}
			case "invalid_value": return e.values.length === 1 ? `অবৈধ ইনপুট: প্রত্যাশিত ${S(e.values[0])}` : `অবৈধ অপশন: ${u(e.values, " | ")} এর মধ্যে একটি প্রত্যাশিত`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `অনেক বড়: ${e.origin ?? "মান"} ${n}${e.maximum.toString()} ${r.unit ?? "এলিমেন্ট"} হতে হবে` : `অনেক বড়: ${e.origin ?? "মান"} ${n}${e.maximum.toString()} হতে হবে`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `অনেক ছোট: ${e.origin} ${n}${e.minimum.toString()} ${r.unit} হতে হবে` : `অনেক ছোট: ${e.origin} ${n}${e.minimum.toString()} হতে হবে`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `অবৈধ স্ট্রিং: "${t.prefix}" দিয়ে শুরু হতে হবে` : t.format === "ends_with" ? `অবৈধ স্ট্রিং: "${t.suffix}" দিয়ে শেষ হতে হবে` : t.format === "includes" ? `অবৈধ স্ট্রিং: "${t.includes}" অন্তর্ভুক্ত থাকতে হবে` : t.format === "regex" ? `অবৈধ স্ট্রিং: ${t.pattern} প্যাটার্ন মিলতে হবে` : `অবৈধ ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `অবৈধ নম্বর: ${e.divisor} এর গুণিতক হতে হবে`;
			case "unrecognized_keys": return `অচেনা কী${e.keys.length > 1 ? "গুলো" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `${e.origin} এ অবৈধ কী`;
			case "invalid_union": return e.options && Array.isArray(e.options) && e.options.length > 0 ? `অবৈধ ডিসক্রিমিনেটর মান। প্রত্যাশিত ${e.options.map((e) => `'${e}'`).join(" | ")}` : "অবৈধ ইনপুট";
			case "invalid_element": return `${e.origin} এ অবৈধ মান`;
			default: return "অবৈধ ইনপুট";
		}
	};
};
function Zo() {
	return { localeError: Xo() };
}
// node_modules/zod/v4/locales/ca.js
var Qo = () => {
	let e = {
		string: {
			unit: "caràcters",
			verb: "contenir"
		},
		file: {
			unit: "bytes",
			verb: "contenir"
		},
		array: {
			unit: "elements",
			verb: "contenir"
		},
		set: {
			unit: "elements",
			verb: "contenir"
		},
		map: {
			unit: "elements",
			verb: "contenir"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "entrada",
		email: "adreça electrònica",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "data i hora ISO",
		date: "data ISO",
		time: "hora ISO",
		duration: "durada ISO",
		ipv4: "adreça IPv4",
		ipv6: "adreça IPv6",
		mac: "adreça MAC",
		cidrv4: "rang IPv4",
		cidrv6: "rang IPv6",
		base64: "cadena codificada en base64",
		base64url: "cadena codificada en base64url",
		json_string: "cadena JSON",
		e164: "número E.164",
		credit_card: "número de targeta de crèdit",
		currency_code: "codi de moneda",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "entrada"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Tipus invàlid: s'esperava instanceof ${e.expected}, s'ha rebut ${i}` : `Tipus invàlid: s'esperava ${t}, s'ha rebut ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Valor invàlid: s'esperava ${S(e.values[0])}` : `Opció invàlida: s'esperava una de ${u(e.values, " o ")}`;
			case "too_big": {
				let n = e.inclusive ? "com a màxim" : "menys de", r = t(e.origin);
				return r ? `Massa gran: s'esperava que ${e.origin ?? "el valor"} contingués ${n} ${e.maximum.toString()} ${r.unit ?? "elements"}` : `Massa gran: s'esperava que ${e.origin ?? "el valor"} fos ${n} ${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? "com a mínim" : "més de", r = t(e.origin);
				return r ? `Massa petit: s'esperava que ${e.origin} contingués ${n} ${e.minimum.toString()} ${r.unit}` : `Massa petit: s'esperava que ${e.origin} fos ${n} ${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Format invàlid: ha de començar amb "${t.prefix}"` : t.format === "ends_with" ? `Format invàlid: ha d'acabar amb "${t.suffix}"` : t.format === "includes" ? `Format invàlid: ha d'incloure "${t.includes}"` : t.format === "regex" ? `Format invàlid: ha de coincidir amb el patró ${t.pattern}` : `Format invàlid per a ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Número invàlid: ha de ser múltiple de ${e.divisor}`;
			case "unrecognized_keys": return `Clau${e.keys.length > 1 ? "s" : ""} no reconeguda${e.keys.length > 1 ? "s" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Clau invàlida a ${e.origin}`;
			case "invalid_union": return "Entrada invàlida";
			case "invalid_element": return `Element invàlid a ${e.origin}`;
			default: return "Entrada invàlida";
		}
	};
};
function $o() {
	return { localeError: Qo() };
}
// node_modules/zod/v4/locales/ckb.js
var es = () => {
	let e = {
		string: {
			unit: "پیت",
			verb: "بێت"
		},
		file: {
			unit: "بایت",
			verb: "بێت"
		},
		array: {
			unit: "دانە",
			verb: "بێت"
		},
		set: {
			unit: "دانە",
			verb: "بێت"
		},
		map: {
			unit: "دانە",
			verb: "بێت"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "regex",
		email: "ئیمەیڵ",
		url: "بەستەر (URL)",
		emoji: "ئیمۆجی",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ڕێکەوت و کات",
		date: "ڕێکەوت",
		time: "کات",
		duration: "ماوە",
		ipv4: "ناونیشانی IPv4",
		ipv6: "ناونیشانی IPv6",
		mac: "ناونیشانی MAC",
		cidrv4: "مەودای IPv4",
		cidrv6: "مەودای IPv6",
		base64: "دەقی base64",
		base64url: "دەقی base64url",
		json_string: "دەقی JSON",
		e164: "ژمارەی E.164",
		credit_card: "ژمارەی کارتی کرێدیت",
		currency_code: "کۆدی دراو",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "تێکردە"
	}, r = {
		nan: "NaN",
		string: "نووسین",
		number: "ژمارە",
		boolean: "boolean",
		array: "array",
		object: "object",
		date: "ڕێکەوت",
		integer: "ژمارە",
		float: "ژمارە",
		null: "null",
		undefined: "undefined",
		function: "function",
		symbol: "symbol",
		unknown: "unknown",
		promise: "promise",
		void: "void",
		never: "never",
		map: "map",
		set: "set"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n, a = [
					"ا",
					"و",
					"ۆ",
					"وو",
					"ە",
					"ی",
					"ێ"
				].some((e) => i.endsWith(e)) ? "یە" : "ە", o = /^[a-zA-Z]+$/.test(i);
				return n === "null" || n === "undefined" ? "داواکراوە" : `چاوەڕوانکراوە ${t} بێت، بەڵام ${i}${o ? "" : a}`;
			}
			case "invalid_value": return e.values.length === 1 ? `بەهاکە نادرووستە: چاوەڕوانکراوە ${S(e.values[0])} بێت` : `هەڵبژاردەی نادروست: چاوەڕوانکراوە یەکێک بێت لە ${u(e.values, "|")}`;
			case "too_big": {
				let n = t(e.origin);
				return n ? `پێویستە بە لایەنی زۆرەوە ${e.maximum.toString()} ${n.unit} ${n.verb}` : `پێویستە بە لایەنی زۆرەوە ${e.maximum.toString()} بێت`;
			}
			case "too_small": {
				let n = t(e.origin);
				return n ? `پێویستە بە لایەنی کەمەوە ${e.minimum.toString()} ${n.unit} ${n.verb}` : `پێویستە بە لایەنی کەمەوە ${e.minimum.toString()} بێت`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `دەقی نادروست: پێویستە دەستپێبکات بە "${t.prefix}"` : t.format === "ends_with" ? `دەقی نادروست: پێویستە کۆتاییبێت بە "${t.suffix}"` : t.format === "includes" ? `دەقی نادروست: پێویستە "${t.includes}" لەخۆبگرێت` : t.format === "regex" ? `دەقی نادروست: پێویستە لەگەڵ پاتێرنی ${t.pattern} بگونجێت` : `بەهای ${n[t.format] ?? e.format} نادروستە`;
			}
			case "not_multiple_of": return `ژمارەی نادروست: دەبێت چەند هێندە بێت بۆ ${e.divisor}`;
			case "unrecognized_keys": return `کلیلی نەناسراو: ${u(e.keys, ", ")}`;
			case "invalid_key": return `کلیلی نادروست لە ${e.origin}`;
			case "invalid_union": return e.options && Array.isArray(e.options) && e.options.length > 0 ? `بەهای نەناسراو هەیە. بەهای چاوەڕوانکراو: ${e.options.map((e) => `'${e}'`).join(" | ")}` : "یەکگرتنی نادروست";
			case "invalid_element": return `${e.origin} بەهاکە نادروستە`;
			default: return "تێکردەی نادروست";
		}
	};
};
function ts() {
	return { localeError: es() };
}
// node_modules/zod/v4/locales/cs.js
var ns = () => {
	let e = {
		string: {
			unit: "znaků",
			verb: "mít"
		},
		file: {
			unit: "bajtů",
			verb: "mít"
		},
		array: {
			unit: "prvků",
			verb: "mít"
		},
		set: {
			unit: "prvků",
			verb: "mít"
		},
		map: {
			unit: "prvků",
			verb: "mít"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "regulární výraz",
		email: "e-mailová adresa",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "datum a čas ve formátu ISO",
		date: "datum ve formátu ISO",
		time: "čas ve formátu ISO",
		duration: "doba trvání ISO",
		ipv4: "IPv4 adresa",
		ipv6: "IPv6 adresa",
		mac: "MAC adresa",
		cidrv4: "rozsah IPv4",
		cidrv6: "rozsah IPv6",
		base64: "řetězec zakódovaný ve formátu base64",
		base64url: "řetězec zakódovaný ve formátu base64url",
		json_string: "řetězec ve formátu JSON",
		e164: "číslo E.164",
		credit_card: "číslo kreditní karty",
		currency_code: "kód měny",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "vstup"
	}, r = {
		nan: "NaN",
		number: "číslo",
		string: "řetězec",
		function: "funkce",
		array: "pole"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Neplatný vstup: očekáváno instanceof ${e.expected}, obdrženo ${i}` : `Neplatný vstup: očekáváno ${t}, obdrženo ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Neplatný vstup: očekáváno ${S(e.values[0])}` : `Neplatná možnost: očekávána jedna z hodnot ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Hodnota je příliš velká: ${e.origin ?? "hodnota"} musí mít ${n}${e.maximum.toString()} ${r.unit ?? "prvků"}` : `Hodnota je příliš velká: ${e.origin ?? "hodnota"} musí být ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Hodnota je příliš malá: ${e.origin ?? "hodnota"} musí mít ${n}${e.minimum.toString()} ${r.unit ?? "prvků"}` : `Hodnota je příliš malá: ${e.origin ?? "hodnota"} musí být ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Neplatný řetězec: musí začínat na "${t.prefix}"` : t.format === "ends_with" ? `Neplatný řetězec: musí končit na "${t.suffix}"` : t.format === "includes" ? `Neplatný řetězec: musí obsahovat "${t.includes}"` : t.format === "regex" ? `Neplatný řetězec: musí odpovídat vzoru ${t.pattern}` : `Neplatný formát ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Neplatné číslo: musí být násobkem ${e.divisor}`;
			case "unrecognized_keys": return `Neznámé klíče: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Neplatný klíč v ${e.origin}`;
			case "invalid_union": return "Neplatný vstup";
			case "invalid_element": return `Neplatná hodnota v ${e.origin}`;
			default: return "Neplatný vstup";
		}
	};
};
function rs() {
	return { localeError: ns() };
}
// node_modules/zod/v4/locales/da.js
var is = () => {
	let e = {
		string: {
			unit: "tegn",
			verb: "havde"
		},
		file: {
			unit: "bytes",
			verb: "havde"
		},
		array: {
			unit: "elementer",
			verb: "indeholdt"
		},
		set: {
			unit: "elementer",
			verb: "indeholdt"
		},
		map: {
			unit: "elementer",
			verb: "indeholdt"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "input",
		email: "e-mailadresse",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO dato- og klokkeslæt",
		date: "ISO-dato",
		time: "ISO-klokkeslæt",
		duration: "ISO-varighed",
		ipv4: "IPv4-adresse",
		ipv6: "IPv6-adresse",
		mac: "MAC-adresse",
		cidrv4: "IPv4-spektrum",
		cidrv6: "IPv6-spektrum",
		base64: "base64-kodet streng",
		base64url: "base64url-kodet streng",
		json_string: "JSON-streng",
		e164: "E.164-nummer",
		credit_card: "kreditkortnummer",
		currency_code: "valutakode",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "input"
	}, r = {
		nan: "NaN",
		string: "streng",
		number: "tal",
		boolean: "boolean",
		array: "liste",
		object: "objekt",
		set: "sæt",
		file: "fil"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Ugyldigt input: forventede instanceof ${e.expected}, fik ${i}` : `Ugyldigt input: forventede ${t}, fik ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Ugyldig værdi: forventede ${S(e.values[0])}` : `Ugyldigt valg: forventede en af følgende ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", i = t(e.origin), a = r[e.origin] ?? e.origin;
				return i ? `For stor: forventede ${a ?? "value"} ${i.verb} ${n} ${e.maximum.toString()} ${i.unit ?? "elementer"}` : `For stor: forventede ${a ?? "value"} havde ${n} ${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", i = t(e.origin), a = r[e.origin] ?? e.origin;
				return i ? `For lille: forventede ${a} ${i.verb} ${n} ${e.minimum.toString()} ${i.unit}` : `For lille: forventede ${a} havde ${n} ${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Ugyldig streng: skal starte med "${t.prefix}"` : t.format === "ends_with" ? `Ugyldig streng: skal ende med "${t.suffix}"` : t.format === "includes" ? `Ugyldig streng: skal indeholde "${t.includes}"` : t.format === "regex" ? `Ugyldig streng: skal matche mønsteret ${t.pattern}` : `Ugyldig ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Ugyldigt tal: skal være deleligt med ${e.divisor}`;
			case "unrecognized_keys": return `${e.keys.length > 1 ? "Ukendte nøgler" : "Ukendt nøgle"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Ugyldig nøgle i ${e.origin}`;
			case "invalid_union": return "Ugyldigt input: matcher ingen af de tilladte typer";
			case "invalid_element": return `Ugyldig værdi i ${e.origin}`;
			default: return "Ugyldigt input";
		}
	};
};
function as() {
	return { localeError: is() };
}
// node_modules/zod/v4/locales/de.js
var os = () => {
	let e = {
		string: {
			unit: "Zeichen",
			verb: "zu haben"
		},
		file: {
			unit: "Bytes",
			verb: "zu haben"
		},
		array: {
			unit: "Elemente",
			verb: "zu haben"
		},
		set: {
			unit: "Elemente",
			verb: "zu haben"
		},
		map: {
			unit: "Elemente",
			verb: "zu haben"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "Eingabe",
		email: "E-Mail-Adresse",
		url: "URL",
		emoji: "Emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO-Datum und -Uhrzeit",
		date: "ISO-Datum",
		time: "ISO-Uhrzeit",
		duration: "ISO-Dauer",
		ipv4: "IPv4-Adresse",
		ipv6: "IPv6-Adresse",
		mac: "MAC-Adresse",
		cidrv4: "IPv4-Bereich",
		cidrv6: "IPv6-Bereich",
		base64: "Base64-codierter String",
		base64url: "Base64-URL-codierter String",
		json_string: "JSON-String",
		e164: "E.164-Nummer",
		credit_card: "Kreditkartennummer",
		currency_code: "Währungscode",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "Eingabe"
	}, r = {
		nan: "NaN",
		number: "Zahl",
		array: "Array"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Ungültige Eingabe: erwartet instanceof ${e.expected}, erhalten ${i}` : `Ungültige Eingabe: erwartet ${t}, erhalten ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Ungültige Eingabe: erwartet ${S(e.values[0])}` : `Ungültige Option: erwartet eine von ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Zu groß: erwartet, dass ${e.origin ?? "Wert"} ${n}${e.maximum.toString()} ${r.unit ?? "Elemente"} hat` : `Zu groß: erwartet, dass ${e.origin ?? "Wert"} ${n}${e.maximum.toString()} ist`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Zu klein: erwartet, dass ${e.origin} ${n}${e.minimum.toString()} ${r.unit} hat` : `Zu klein: erwartet, dass ${e.origin} ${n}${e.minimum.toString()} ist`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Ungültiger String: muss mit "${t.prefix}" beginnen` : t.format === "ends_with" ? `Ungültiger String: muss mit "${t.suffix}" enden` : t.format === "includes" ? `Ungültiger String: muss "${t.includes}" enthalten` : t.format === "regex" ? `Ungültiger String: muss dem Muster ${t.pattern} entsprechen` : `Ungültig: ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Ungültige Zahl: muss ein Vielfaches von ${e.divisor} sein`;
			case "unrecognized_keys": return `${e.keys.length > 1 ? "Unbekannte Schlüssel" : "Unbekannter Schlüssel"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Ungültiger Schlüssel in ${e.origin}`;
			case "invalid_union": return "Ungültige Eingabe";
			case "invalid_element": return `Ungültiger Wert in ${e.origin}`;
			default: return "Ungültige Eingabe";
		}
	};
};
function ss() {
	return { localeError: os() };
}
// node_modules/zod/v4/locales/el.js
var cs = () => {
	let e = {
		string: {
			unit: "χαρακτήρες",
			verb: "να έχει"
		},
		file: {
			unit: "bytes",
			verb: "να έχει"
		},
		array: {
			unit: "στοιχεία",
			verb: "να έχει"
		},
		set: {
			unit: "στοιχεία",
			verb: "να έχει"
		},
		map: {
			unit: "καταχωρήσεις",
			verb: "να έχει"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "είσοδος",
		email: "διεύθυνση email",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO ημερομηνία και ώρα",
		date: "ISO ημερομηνία",
		time: "ISO ώρα",
		duration: "ISO διάρκεια",
		ipv4: "διεύθυνση IPv4",
		ipv6: "διεύθυνση IPv6",
		mac: "διεύθυνση MAC",
		cidrv4: "εύρος IPv4",
		cidrv6: "εύρος IPv6",
		base64: "συμβολοσειρά κωδικοποιημένη σε base64",
		base64url: "συμβολοσειρά κωδικοποιημένη σε base64url",
		json_string: "συμβολοσειρά JSON",
		e164: "αριθμός E.164",
		credit_card: "αριθμός πιστωτικής κάρτας",
		currency_code: "κωδικός νομίσματος",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "είσοδος"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return typeof e.expected == "string" && /^[A-Z]/.test(e.expected) ? `Μη έγκυρη είσοδος: αναμενόταν instanceof ${e.expected}, λήφθηκε ${i}` : `Μη έγκυρη είσοδος: αναμενόταν ${t}, λήφθηκε ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Μη έγκυρη είσοδος: αναμενόταν ${S(e.values[0])}` : `Μη έγκυρη επιλογή: αναμενόταν ένα από ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Πολύ μεγάλο: αναμενόταν ${e.origin ?? "τιμή"} να έχει ${n}${e.maximum.toString()} ${r.unit ?? "στοιχεία"}` : `Πολύ μεγάλο: αναμενόταν ${e.origin ?? "τιμή"} να είναι ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Πολύ μικρό: αναμενόταν ${e.origin} να έχει ${n}${e.minimum.toString()} ${r.unit}` : `Πολύ μικρό: αναμενόταν ${e.origin} να είναι ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Μη έγκυρη συμβολοσειρά: πρέπει να ξεκινά με "${t.prefix}"` : t.format === "ends_with" ? `Μη έγκυρη συμβολοσειρά: πρέπει να τελειώνει με "${t.suffix}"` : t.format === "includes" ? `Μη έγκυρη συμβολοσειρά: πρέπει να περιέχει "${t.includes}"` : t.format === "regex" ? `Μη έγκυρη συμβολοσειρά: πρέπει να ταιριάζει με το μοτίβο ${t.pattern}` : `Μη έγκυρο: ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Μη έγκυρος αριθμός: πρέπει να είναι πολλαπλάσιο του ${e.divisor}`;
			case "unrecognized_keys": return `Άγνωστ${e.keys.length > 1 ? "α" : "ο"} κλειδ${e.keys.length > 1 ? "ιά" : "ί"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Μη έγκυρο κλειδί στο ${e.origin}`;
			case "invalid_union": return "Μη έγκυρη είσοδος";
			case "invalid_element": return `Μη έγκυρη τιμή στο ${e.origin}`;
			default: return "Μη έγκυρη είσοδος";
		}
	};
};
function ls() {
	return { localeError: cs() };
}
// node_modules/zod/v4/locales/en.js
var us = () => {
	let e = {
		string: {
			unit: "characters",
			verb: "to have"
		},
		file: {
			unit: "bytes",
			verb: "to have"
		},
		array: {
			unit: "items",
			verb: "to have"
		},
		set: {
			unit: "items",
			verb: "to have"
		},
		map: {
			unit: "entries",
			verb: "to have"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "input",
		email: "email address",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO datetime",
		date: "ISO date",
		time: "ISO time",
		duration: "ISO duration",
		ipv4: "IPv4 address",
		ipv6: "IPv6 address",
		mac: "MAC address",
		cidrv4: "IPv4 range",
		cidrv6: "IPv6 range",
		base64: "base64-encoded string",
		base64url: "base64url-encoded string",
		json_string: "JSON string",
		e164: "E.164 number",
		currency_code: "currency code",
		credit_card: "credit card number",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "input"
	}, r = { nan: "NaN" };
	function i(e, t) {
		return e === "number" && typeof t == "number" && !Number.isFinite(t) ? String(t) : r[e] ?? e;
	}
	return (e) => {
		switch (e.code) {
			case "invalid_type": return `Invalid input: expected ${i(e.expected)}, received ${i(E(e.input), e.input)}`;
			case "invalid_value": return e.values.length === 1 ? `Invalid input: expected ${S(e.values[0])}` : `Invalid option: expected one of ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.exact ? "exactly " : e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Too big: expected ${e.origin ?? "value"} to have ${n}${e.maximum.toString()} ${r.unit ?? "elements"}` : `Too big: expected ${e.origin ?? "value"} to be ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.exact ? "exactly " : e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Too small: expected ${e.origin} to have ${n}${e.minimum.toString()} ${r.unit}` : `Too small: expected ${e.origin} to be ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Invalid string: must start with "${t.prefix}"` : t.format === "ends_with" ? `Invalid string: must end with "${t.suffix}"` : t.format === "includes" ? `Invalid string: must include "${t.includes}"` : t.format === "regex" ? `Invalid string: must match pattern ${t.pattern}` : `Invalid ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Invalid number: must be a multiple of ${e.divisor}`;
			case "unrecognized_keys": return `Unrecognized key${e.keys.length > 1 ? "s" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Invalid key in ${e.origin}`;
			case "invalid_union": return e.options && Array.isArray(e.options) && e.options.length > 0 ? `Invalid discriminator value. Expected ${e.options.map((e) => `'${e}'`).join(" | ")}` : e.inclusive === !1 ? "Invalid input: more than one option matched" : "Invalid input";
			case "invalid_element": return `Invalid value in ${e.origin}`;
			default: return "Invalid input";
		}
	};
};
function ds() {
	return { localeError: us() };
}
// node_modules/zod/v4/locales/eo.js
var fs = () => {
	let e = {
		string: {
			unit: "karaktrojn",
			verb: "havi"
		},
		file: {
			unit: "bajtojn",
			verb: "havi"
		},
		array: {
			unit: "elementojn",
			verb: "havi"
		},
		set: {
			unit: "elementojn",
			verb: "havi"
		},
		map: {
			unit: "elementojn",
			verb: "havi"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "enigo",
		email: "retadreso",
		url: "URL",
		emoji: "emoĝio",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO-datotempo",
		date: "ISO-dato",
		time: "ISO-tempo",
		duration: "ISO-daŭro",
		ipv4: "IPv4-adreso",
		ipv6: "IPv6-adreso",
		mac: "MAC-adreso",
		cidrv4: "IPv4-rango",
		cidrv6: "IPv6-rango",
		base64: "64-ume kodita karaktraro",
		base64url: "URL-64-ume kodita karaktraro",
		json_string: "JSON-karaktraro",
		e164: "E.164-nombro",
		credit_card: "kreditkarta numero",
		currency_code: "valuta kodo",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "enigo"
	}, r = {
		nan: "NaN",
		number: "nombro",
		array: "tabelo",
		null: "senvalora"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Nevalida enigo: atendiĝis instanceof ${e.expected}, riceviĝis ${i}` : `Nevalida enigo: atendiĝis ${t}, riceviĝis ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Nevalida enigo: atendiĝis ${S(e.values[0])}` : `Nevalida opcio: atendiĝis unu el ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Tro granda: atendiĝis ke ${e.origin ?? "valoro"} havu ${n}${e.maximum.toString()} ${r.unit ?? "elementojn"}` : `Tro granda: atendiĝis ke ${e.origin ?? "valoro"} havu ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Tro malgranda: atendiĝis ke ${e.origin} havu ${n}${e.minimum.toString()} ${r.unit}` : `Tro malgranda: atendiĝis ke ${e.origin} estu ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Nevalida karaktraro: devas komenciĝi per "${t.prefix}"` : t.format === "ends_with" ? `Nevalida karaktraro: devas finiĝi per "${t.suffix}"` : t.format === "includes" ? `Nevalida karaktraro: devas inkluzivi "${t.includes}"` : t.format === "regex" ? `Nevalida karaktraro: devas kongrui kun la modelo ${t.pattern}` : `Nevalida ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Nevalida nombro: devas esti oblo de ${e.divisor}`;
			case "unrecognized_keys": return `Nekonata${e.keys.length > 1 ? "j" : ""} ŝlosilo${e.keys.length > 1 ? "j" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Nevalida ŝlosilo en ${e.origin}`;
			case "invalid_union": return "Nevalida enigo";
			case "invalid_element": return `Nevalida valoro en ${e.origin}`;
			default: return "Nevalida enigo";
		}
	};
};
function ps() {
	return { localeError: fs() };
}
// node_modules/zod/v4/locales/es.js
var ms = () => {
	let e = {
		string: {
			unit: "caracteres",
			verb: "tener"
		},
		file: {
			unit: "bytes",
			verb: "tener"
		},
		array: {
			unit: "elementos",
			verb: "tener"
		},
		set: {
			unit: "elementos",
			verb: "tener"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "entrada",
		email: "dirección de correo electrónico",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "fecha y hora ISO",
		date: "fecha ISO",
		time: "hora ISO",
		duration: "duración ISO",
		ipv4: "dirección IPv4",
		ipv6: "dirección IPv6",
		mac: "dirección MAC",
		cidrv4: "rango IPv4",
		cidrv6: "rango IPv6",
		base64: "cadena codificada en base64",
		base64url: "URL codificada en base64",
		json_string: "cadena JSON",
		e164: "número E.164",
		credit_card: "número de tarjeta de crédito",
		currency_code: "código de moneda",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "entrada"
	}, r = {
		nan: "NaN",
		string: "texto",
		number: "número",
		boolean: "booleano",
		array: "arreglo",
		object: "objeto",
		set: "conjunto",
		file: "archivo",
		date: "fecha",
		bigint: "número grande",
		symbol: "símbolo",
		undefined: "indefinido",
		null: "nulo",
		function: "función",
		map: "mapa",
		record: "registro",
		tuple: "tupla",
		enum: "enumeración",
		union: "unión",
		literal: "literal",
		promise: "promesa",
		void: "vacío",
		never: "nunca",
		unknown: "desconocido",
		any: "cualquiera"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Entrada inválida: se esperaba instanceof ${e.expected}, recibido ${i}` : `Entrada inválida: se esperaba ${t}, recibido ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Entrada inválida: se esperaba ${S(e.values[0])}` : `Opción inválida: se esperaba una de ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", i = t(e.origin), a = r[e.origin] ?? e.origin;
				return i ? `Demasiado grande: se esperaba que ${a ?? "valor"} tuviera ${n}${e.maximum.toString()} ${i.unit ?? "elementos"}` : `Demasiado grande: se esperaba que ${a ?? "valor"} fuera ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", i = t(e.origin), a = r[e.origin] ?? e.origin;
				return i ? `Demasiado pequeño: se esperaba que ${a} tuviera ${n}${e.minimum.toString()} ${i.unit}` : `Demasiado pequeño: se esperaba que ${a} fuera ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Cadena inválida: debe comenzar con "${t.prefix}"` : t.format === "ends_with" ? `Cadena inválida: debe terminar en "${t.suffix}"` : t.format === "includes" ? `Cadena inválida: debe incluir "${t.includes}"` : t.format === "regex" ? `Cadena inválida: debe coincidir con el patrón ${t.pattern}` : `Inválido ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Número inválido: debe ser múltiplo de ${e.divisor}`;
			case "unrecognized_keys": return `Llave${e.keys.length > 1 ? "s" : ""} desconocida${e.keys.length > 1 ? "s" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Llave inválida en ${r[e.origin] ?? e.origin}`;
			case "invalid_union": return "Entrada inválida";
			case "invalid_element": return `Valor inválido en ${r[e.origin] ?? e.origin}`;
			default: return "Entrada inválida";
		}
	};
};
function hs() {
	return { localeError: ms() };
}
// node_modules/zod/v4/locales/fa.js
var gs = () => {
	let e = {
		string: {
			unit: "کاراکتر",
			verb: "داشته باشد"
		},
		file: {
			unit: "بایت",
			verb: "داشته باشد"
		},
		array: {
			unit: "آیتم",
			verb: "داشته باشد"
		},
		set: {
			unit: "آیتم",
			verb: "داشته باشد"
		},
		map: {
			unit: "آیتم",
			verb: "داشته باشد"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "ورودی",
		email: "آدرس ایمیل",
		url: "URL",
		emoji: "ایموجی",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "تاریخ و زمان ایزو",
		date: "تاریخ ایزو",
		time: "زمان ایزو",
		duration: "مدت زمان ایزو",
		ipv4: "IPv4 آدرس",
		ipv6: "IPv6 آدرس",
		mac: "MAC آدرس",
		cidrv4: "IPv4 دامنه",
		cidrv6: "IPv6 دامنه",
		base64: "base64-encoded رشته",
		base64url: "base64url-encoded رشته",
		json_string: "JSON رشته",
		e164: "E.164 عدد",
		credit_card: "شماره کارت اعتباری",
		currency_code: "کد ارز",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "ورودی"
	}, r = {
		nan: "NaN",
		number: "عدد",
		array: "آرایه"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `ورودی نامعتبر: می‌بایست instanceof ${e.expected} می‌بود، ${i} دریافت شد` : `ورودی نامعتبر: می‌بایست ${t} می‌بود، ${i} دریافت شد`;
			}
			case "invalid_value": return e.values.length === 1 ? `ورودی نامعتبر: می‌بایست ${S(e.values[0])} می‌بود` : `گزینه نامعتبر: می‌بایست یکی از ${u(e.values, "|")} می‌بود`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `خیلی بزرگ: ${e.origin ?? "مقدار"} باید ${n}${e.maximum.toString()} ${r.unit ?? "عنصر"} باشد` : `خیلی بزرگ: ${e.origin ?? "مقدار"} باید ${n}${e.maximum.toString()} باشد`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `خیلی کوچک: ${e.origin} باید ${n}${e.minimum.toString()} ${r.unit} باشد` : `خیلی کوچک: ${e.origin} باید ${n}${e.minimum.toString()} باشد`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `رشته نامعتبر: باید با "${t.prefix}" شروع شود` : t.format === "ends_with" ? `رشته نامعتبر: باید با "${t.suffix}" تمام شود` : t.format === "includes" ? `رشته نامعتبر: باید شامل "${t.includes}" باشد` : t.format === "regex" ? `رشته نامعتبر: باید با الگوی ${t.pattern} مطابقت داشته باشد` : `${n[t.format] ?? e.format} نامعتبر`;
			}
			case "not_multiple_of": return `عدد نامعتبر: باید مضرب ${e.divisor} باشد`;
			case "unrecognized_keys": return `کلید${e.keys.length > 1 ? "های" : ""} ناشناس: ${u(e.keys, ", ")}`;
			case "invalid_key": return `کلید ناشناس در ${e.origin}`;
			case "invalid_union": return "ورودی نامعتبر";
			case "invalid_element": return `مقدار نامعتبر در ${e.origin}`;
			default: return "ورودی نامعتبر";
		}
	};
};
function _s() {
	return { localeError: gs() };
}
// node_modules/zod/v4/locales/fi.js
var vs = () => {
	let e = {
		string: {
			unit: "merkkiä",
			subject: "merkkijonon"
		},
		file: {
			unit: "tavua",
			subject: "tiedoston"
		},
		array: {
			unit: "alkiota",
			subject: "listan"
		},
		set: {
			unit: "alkiota",
			subject: "joukon"
		},
		map: {
			unit: "alkiota",
			subject: "kuvauksen"
		},
		number: {
			unit: "",
			subject: "luvun"
		},
		bigint: {
			unit: "",
			subject: "suuren kokonaisluvun"
		},
		int: {
			unit: "",
			subject: "kokonaisluvun"
		},
		date: {
			unit: "",
			subject: "päivämäärän"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "säännöllinen lauseke",
		email: "sähköpostiosoite",
		url: "URL-osoite",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO-aikaleima",
		date: "ISO-päivämäärä",
		time: "ISO-aika",
		duration: "ISO-kesto",
		ipv4: "IPv4-osoite",
		ipv6: "IPv6-osoite",
		mac: "MAC-osoite",
		cidrv4: "IPv4-alue",
		cidrv6: "IPv6-alue",
		base64: "base64-koodattu merkkijono",
		base64url: "base64url-koodattu merkkijono",
		json_string: "JSON-merkkijono",
		e164: "E.164-luku",
		credit_card: "luottokortin numero",
		currency_code: "valuuttakoodi",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "templaattimerkkijono"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Virheellinen tyyppi: odotettiin instanceof ${e.expected}, oli ${i}` : `Virheellinen tyyppi: odotettiin ${t}, oli ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Virheellinen syöte: täytyy olla ${S(e.values[0])}` : `Virheellinen valinta: täytyy olla yksi seuraavista: ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Liian suuri: ${r.subject} täytyy olla ${n}${e.maximum.toString()} ${r.unit}`.trim() : `Liian suuri: arvon täytyy olla ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Liian pieni: ${r.subject} täytyy olla ${n}${e.minimum.toString()} ${r.unit}`.trim() : `Liian pieni: arvon täytyy olla ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Virheellinen syöte: täytyy alkaa "${t.prefix}"` : t.format === "ends_with" ? `Virheellinen syöte: täytyy loppua "${t.suffix}"` : t.format === "includes" ? `Virheellinen syöte: täytyy sisältää "${t.includes}"` : t.format === "regex" ? `Virheellinen syöte: täytyy vastata säännöllistä lauseketta ${t.pattern}` : `Virheellinen ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Virheellinen luku: täytyy olla luvun ${e.divisor} monikerta`;
			case "unrecognized_keys": return `${e.keys.length > 1 ? "Tuntemattomat avaimet" : "Tuntematon avain"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return "Virheellinen avain tietueessa";
			case "invalid_union": return "Virheellinen unioni";
			case "invalid_element": return "Virheellinen arvo joukossa";
			default: return "Virheellinen syöte";
		}
	};
};
function ys() {
	return { localeError: vs() };
}
// node_modules/zod/v4/locales/fr.js
var bs = () => {
	let e = {
		string: {
			unit: "caractères",
			verb: "avoir"
		},
		file: {
			unit: "octets",
			verb: "avoir"
		},
		array: {
			unit: "éléments",
			verb: "avoir"
		},
		set: {
			unit: "éléments",
			verb: "avoir"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "expression régulière",
		email: "adresse e-mail",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "date et heure ISO",
		date: "date ISO",
		time: "heure ISO",
		duration: "durée ISO",
		ipv4: "adresse IPv4",
		ipv6: "adresse IPv6",
		mac: "adresse MAC",
		cidrv4: "plage IPv4",
		cidrv6: "plage IPv6",
		base64: "chaîne de caractères encodée en base64",
		base64url: "chaîne de caractères encodée en base64url",
		json_string: "chaîne de caractères JSON",
		e164: "numéro au format E.164",
		credit_card: "numéro de carte de crédit",
		currency_code: "code de devise",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "entrée"
	}, r = {
		string: "chaîne de caractères",
		number: "nombre",
		int: "entier",
		boolean: "booléen",
		bigint: "grand entier",
		symbol: "symbole",
		undefined: "indéfini",
		null: "null",
		never: "jamais",
		void: "vide",
		date: "date",
		array: "tableau",
		object: "objet",
		tuple: "tuple",
		record: "record",
		map: "map",
		set: "ensemble",
		file: "fichier",
		nonoptional: "non optionnel",
		nan: "NaN",
		function: "fonction"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Entrée invalide : instance de ${e.expected} attendu, ${i} reçu` : `Entrée invalide : ${t} attendu, ${i} reçu`;
			}
			case "invalid_value": return e.values.length === 1 ? `Entrée invalide : ${S(e.values[0])} attendu` : `Option invalide : une valeur parmi ${u(e.values, "|")} attendue`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", i = t(e.origin);
				return i ? `Trop grand : ${r[e.origin] ?? "valeur"} doit ${i.verb} ${n}${e.maximum.toString()} ${i.unit ?? "élément(s)"}` : `Trop grand : ${r[e.origin] ?? "valeur"} doit être ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", i = t(e.origin);
				return i ? `Trop petit : ${r[e.origin] ?? "valeur"} doit ${i.verb} ${n}${e.minimum.toString()} ${i.unit}` : `Trop petit : ${r[e.origin] ?? "valeur"} doit être ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Chaîne de caractères invalide : doit commencer par "${t.prefix}"` : t.format === "ends_with" ? `Chaîne de caractères invalide : doit se terminer par "${t.suffix}"` : t.format === "includes" ? `Chaîne de caractères invalide : doit inclure "${t.includes}"` : t.format === "regex" ? `Chaîne de caractères invalide : doit correspondre au motif ${t.pattern}` : `${n[t.format] ?? e.format} invalide`;
			}
			case "not_multiple_of": return `Nombre invalide : doit être un multiple de ${e.divisor}`;
			case "unrecognized_keys": return `Clé${e.keys.length > 1 ? "s" : ""} non reconnue${e.keys.length > 1 ? "s" : ""} : ${u(e.keys, ", ")}`;
			case "invalid_key": return `Clé invalide dans ${e.origin}`;
			case "invalid_union": return "Entrée invalide";
			case "invalid_element": return `Valeur invalide dans ${e.origin}`;
			default: return "Entrée invalide";
		}
	};
};
function xs() {
	return { localeError: bs() };
}
// node_modules/zod/v4/locales/fr-CA.js
var Ss = () => {
	let e = {
		string: {
			unit: "caractères",
			verb: "avoir"
		},
		file: {
			unit: "octets",
			verb: "avoir"
		},
		array: {
			unit: "éléments",
			verb: "avoir"
		},
		set: {
			unit: "éléments",
			verb: "avoir"
		},
		map: {
			unit: "éléments",
			verb: "avoir"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "entrée",
		email: "adresse courriel",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "date-heure ISO",
		date: "date ISO",
		time: "heure ISO",
		duration: "durée ISO",
		ipv4: "adresse IPv4",
		ipv6: "adresse IPv6",
		mac: "adresse MAC",
		cidrv4: "plage IPv4",
		cidrv6: "plage IPv6",
		base64: "chaîne encodée en base64",
		base64url: "chaîne encodée en base64url",
		json_string: "chaîne JSON",
		e164: "numéro E.164",
		credit_card: "numéro de carte de crédit",
		currency_code: "code de devise",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "entrée"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Entrée invalide : attendu instanceof ${e.expected}, reçu ${i}` : `Entrée invalide : attendu ${t}, reçu ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Entrée invalide : attendu ${S(e.values[0])}` : `Option invalide : attendu l'une des valeurs suivantes ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "≤" : "<", r = t(e.origin);
				return r ? `Trop grand : attendu que ${e.origin ?? "la valeur"} ait ${n}${e.maximum.toString()} ${r.unit}` : `Trop grand : attendu que ${e.origin ?? "la valeur"} soit ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? "≥" : ">", r = t(e.origin);
				return r ? `Trop petit : attendu que ${e.origin} ait ${n}${e.minimum.toString()} ${r.unit}` : `Trop petit : attendu que ${e.origin} soit ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Chaîne invalide : doit commencer par "${t.prefix}"` : t.format === "ends_with" ? `Chaîne invalide : doit se terminer par "${t.suffix}"` : t.format === "includes" ? `Chaîne invalide : doit inclure "${t.includes}"` : t.format === "regex" ? `Chaîne invalide : doit correspondre au motif ${t.pattern}` : `${n[t.format] ?? e.format} invalide`;
			}
			case "not_multiple_of": return `Nombre invalide : doit être un multiple de ${e.divisor}`;
			case "unrecognized_keys": return `Clé${e.keys.length > 1 ? "s" : ""} non reconnue${e.keys.length > 1 ? "s" : ""} : ${u(e.keys, ", ")}`;
			case "invalid_key": return `Clé invalide dans ${e.origin}`;
			case "invalid_union": return "Entrée invalide";
			case "invalid_element": return `Valeur invalide dans ${e.origin}`;
			default: return "Entrée invalide";
		}
	};
};
function Cs() {
	return { localeError: Ss() };
}
// node_modules/zod/v4/locales/gu.js
var ws = () => {
	let e = {
		string: {
			unit: "અક્ષર",
			verb: "હોવા જોઈએ"
		},
		file: {
			unit: "બાયટ",
			verb: "હોવા જોઈએ"
		},
		array: {
			unit: "આઇટમ",
			verb: "હોવા જોઈએ"
		},
		set: {
			unit: "આઇટમ",
			verb: "હોવા જોઈએ"
		},
		map: {
			unit: "એન્ટ્રી",
			verb: "હોવા જોઈએ"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "ઇનપુટ",
		email: "ઈમેઇલ એડ્રેસ",
		url: "URL",
		emoji: "ઇમોજી",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO તારીખ અને સમય",
		date: "ISO તારીખ",
		time: "ISO સમય",
		duration: "ISO અવધિ",
		ipv4: "IPv4 એડ્રેસ",
		ipv6: "IPv6 એડ્રેસ",
		mac: "MAC એડ્રેસ",
		cidrv4: "IPv4 શ્રેણી",
		cidrv6: "IPv6 શ્રેણી",
		base64: "base64-એન્કોડેડ સ્ટ્રિંગ",
		base64url: "base64url-એન્કોડેડ સ્ટ્રિંગ",
		json_string: "JSON સ્ટ્રિંગ",
		e164: "E.164 નંબર",
		credit_card: "ક્રેડિટ કાર્ડ નંબર",
		currency_code: "ચલણ કોડ",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "ઇનપુટ"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input);
				return `અમાન્ય ઇનપુટ: અપેક્ષિત ${t}, પ્રાપ્ત ${r[n] ?? n}`;
			}
			case "invalid_value": return e.values.length === 1 ? `અમાન્ય ઇનપુટ: અપેક્ષિત ${S(e.values[0])}` : `અમાન્ય વિકલ્પ: ${u(e.values, " | ")} માધ્યમથી એક અપેક્ષિત`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `ખૂબ મોટું: ${e.origin ?? "મૂલ્ય"} ${n}${e.maximum.toString()} ${r.unit ?? "એલિમેન્ટ"} હોવા જોઈએ` : `ખૂબ મોટું: ${e.origin ?? "મૂલ્ય"} ${n}${e.maximum.toString()} હોવું જોઈએ`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `ખૂબ નાનું: ${e.origin} ${n}${e.minimum.toString()} ${r.unit} હોવા જોઈએ` : `ખૂબ નાનું: ${e.origin} ${n}${e.minimum.toString()} હોવું જોઈએ`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `અમાન્ય સ્ટ્રિંગ: "${t.prefix}" થી શરૂ થવું જોઈએ` : t.format === "ends_with" ? `અમાન્ય સ્ટ્રિંગ: "${t.suffix}" પર સમાપ્ત થવું જોઈએ` : t.format === "includes" ? `અમાન્ય સ્ટ્રિંગ: "${t.includes}" શામેલ હોવું જોઈએ` : t.format === "regex" ? `અમાન્ય સ્ટ્રિંગ: પેટર્ન ${t.pattern} સાથે મેળ ખાવું જોઈએ` : `અમાન્ય ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `અમાન્ય નંબર: ${e.divisor} નો ગુણાંક હોવો જોઈએ`;
			case "unrecognized_keys": return `ઓળખી શકાતા નહીં તે કી${e.keys.length > 1 ? "ઓ" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `${e.origin} માં અમાન્ય કી`;
			case "invalid_union": return e.options && Array.isArray(e.options) && e.options.length > 0 ? `અમાન્ય ડિસ્ક્રિમિનેટર મૂલ્ય. અપેક્ષિત ${e.options.map((e) => `'${e}'`).join(" | ")}` : "અમાન્ય ઇનપુટ";
			case "invalid_element": return `${e.origin} માં અમાન્ય મૂલ્ય`;
			default: return "અમાન્ય ઇનપુટ";
		}
	};
};
function Ts() {
	return { localeError: ws() };
}
// node_modules/zod/v4/locales/he.js
var Es = () => {
	let e = {
		string: {
			label: "מחרוזת",
			gender: "f"
		},
		number: {
			label: "מספר",
			gender: "m"
		},
		boolean: {
			label: "ערך בוליאני",
			gender: "m"
		},
		bigint: {
			label: "BigInt",
			gender: "m"
		},
		date: {
			label: "תאריך",
			gender: "m"
		},
		array: {
			label: "מערך",
			gender: "m"
		},
		object: {
			label: "אובייקט",
			gender: "m"
		},
		null: {
			label: "ערך ריק (null)",
			gender: "m"
		},
		undefined: {
			label: "ערך לא מוגדר (undefined)",
			gender: "m"
		},
		symbol: {
			label: "סימבול (Symbol)",
			gender: "m"
		},
		function: {
			label: "פונקציה",
			gender: "f"
		},
		map: {
			label: "מפה (Map)",
			gender: "f"
		},
		set: {
			label: "קבוצה (Set)",
			gender: "f"
		},
		file: {
			label: "קובץ",
			gender: "m"
		},
		promise: {
			label: "Promise",
			gender: "m"
		},
		NaN: {
			label: "NaN",
			gender: "m"
		},
		unknown: {
			label: "ערך לא ידוע",
			gender: "m"
		},
		value: {
			label: "ערך",
			gender: "m"
		}
	}, t = {
		string: {
			unit: "תווים",
			shortLabel: "קצר",
			longLabel: "ארוך"
		},
		file: {
			unit: "בייטים",
			shortLabel: "קטן",
			longLabel: "גדול"
		},
		array: {
			unit: "פריטים",
			shortLabel: "קטן",
			longLabel: "גדול"
		},
		set: {
			unit: "פריטים",
			shortLabel: "קטן",
			longLabel: "גדול"
		},
		number: {
			unit: "",
			shortLabel: "קטן",
			longLabel: "גדול"
		}
	}, n = (t) => t ? e[t] : void 0, r = (t) => {
		let r = n(t);
		return r ? r.label : t ?? e.unknown.label;
	}, i = (e) => `ה${r(e)}`, a = (e) => (n(e)?.gender ?? "m") === "f" ? "צריכה להיות" : "צריך להיות", o = (e) => e ? t[e] ?? null : null, s = {
		regex: {
			label: "קלט",
			gender: "m"
		},
		email: {
			label: "כתובת אימייל",
			gender: "f"
		},
		url: {
			label: "כתובת רשת",
			gender: "f"
		},
		emoji: {
			label: "אימוג'י",
			gender: "m"
		},
		uuid: {
			label: "UUID",
			gender: "m"
		},
		uuidv4: {
			label: "UUIDv4",
			gender: "m"
		},
		uuidv6: {
			label: "UUIDv6",
			gender: "m"
		},
		nanoid: {
			label: "nanoid",
			gender: "m"
		},
		guid: {
			label: "GUID",
			gender: "m"
		},
		cuid: {
			label: "cuid",
			gender: "m"
		},
		cuid2: {
			label: "cuid2",
			gender: "m"
		},
		ulid: {
			label: "ULID",
			gender: "m"
		},
		xid: {
			label: "XID",
			gender: "m"
		},
		ksuid: {
			label: "KSUID",
			gender: "m"
		},
		datetime: {
			label: "תאריך וזמן ISO",
			gender: "m"
		},
		date: {
			label: "תאריך ISO",
			gender: "m"
		},
		time: {
			label: "זמן ISO",
			gender: "m"
		},
		duration: {
			label: "משך זמן ISO",
			gender: "m"
		},
		ipv4: {
			label: "כתובת IPv4",
			gender: "f"
		},
		ipv6: {
			label: "כתובת IPv6",
			gender: "f"
		},
		mac: {
			label: "כתובת MAC",
			gender: "f"
		},
		cidrv4: {
			label: "טווח IPv4",
			gender: "m"
		},
		cidrv6: {
			label: "טווח IPv6",
			gender: "m"
		},
		base64: {
			label: "מחרוזת בבסיס 64",
			gender: "f"
		},
		base64url: {
			label: "מחרוזת בבסיס 64 לכתובות רשת",
			gender: "f"
		},
		json_string: {
			label: "מחרוזת JSON",
			gender: "f"
		},
		e164: {
			label: "מספר E.164",
			gender: "m"
		},
		credit_card: {
			label: "מספר כרטיס אשראי",
			gender: "m"
		},
		currency_code: {
			label: "קוד מטבע",
			gender: "m"
		},
		iban: {
			label: "IBAN",
			gender: "m"
		},
		jwt: {
			label: "JWT",
			gender: "m"
		},
		template_literal: {
			label: "קלט",
			gender: "m"
		},
		ends_with: {
			label: "קלט",
			gender: "m"
		},
		includes: {
			label: "קלט",
			gender: "m"
		},
		lowercase: {
			label: "קלט",
			gender: "m"
		},
		starts_with: {
			label: "קלט",
			gender: "m"
		},
		uppercase: {
			label: "קלט",
			gender: "m"
		}
	}, c = { nan: "NaN" };
	return (t) => {
		switch (t.code) {
			case "invalid_type": {
				let n = t.expected, i = c[n ?? ""] ?? r(n), a = E(t.input), o = c[a] ?? e[a]?.label ?? a;
				return /^[A-Z]/.test(t.expected) ? `קלט לא תקין: צריך להיות instanceof ${t.expected}, התקבל ${o}` : `קלט לא תקין: צריך להיות ${i}, התקבל ${o}`;
			}
			case "invalid_value": {
				if (t.values.length === 1) return `ערך לא תקין: הערך חייב להיות ${S(t.values[0])}`;
				let e = t.values.map((e) => S(e));
				if (t.values.length === 2) return `ערך לא תקין: האפשרויות המתאימות הן ${e[0]} או ${e[1]}`;
				let n = e[e.length - 1];
				return `ערך לא תקין: האפשרויות המתאימות הן ${e.slice(0, -1).join(", ")} או ${n}`;
			}
			case "too_big": {
				let e = o(t.origin), n = i(t.origin ?? "value");
				if (t.origin === "string") return `${e?.longLabel ?? "ארוך"} מדי: ${n} צריכה להכיל ${t.maximum.toString()} ${e?.unit ?? ""} ${t.inclusive ? "או פחות" : "לכל היותר"}`.trim();
				if (t.origin === "number") return `גדול מדי: ${n} צריך להיות ${t.inclusive ? `קטן או שווה ל-${t.maximum}` : `קטן מ-${t.maximum}`}`;
				if (t.origin === "array" || t.origin === "set") return `גדול מדי: ${n} ${t.origin === "set" ? "צריכה" : "צריך"} להכיל ${t.inclusive ? `${t.maximum} ${e?.unit ?? ""} או פחות` : `פחות מ-${t.maximum} ${e?.unit ?? ""}`}`.trim();
				let r = t.inclusive ? "<=" : "<", s = a(t.origin ?? "value");
				return e?.unit ? `${e.longLabel} מדי: ${n} ${s} ${r}${t.maximum.toString()} ${e.unit}` : `${e?.longLabel ?? "גדול"} מדי: ${n} ${s} ${r}${t.maximum.toString()}`;
			}
			case "too_small": {
				let e = o(t.origin), n = i(t.origin ?? "value");
				if (t.origin === "string") return `${e?.shortLabel ?? "קצר"} מדי: ${n} צריכה להכיל ${t.minimum.toString()} ${e?.unit ?? ""} ${t.inclusive ? "או יותר" : "לפחות"}`.trim();
				if (t.origin === "number") return `קטן מדי: ${n} צריך להיות ${t.inclusive ? `גדול או שווה ל-${t.minimum}` : `גדול מ-${t.minimum}`}`;
				if (t.origin === "array" || t.origin === "set") {
					let r = t.origin === "set" ? "צריכה" : "צריך";
					return t.minimum === 1 && t.inclusive ? `קטן מדי: ${n} ${r} להכיל ${t.origin, "לפחות פריט אחד"}` : `קטן מדי: ${n} ${r} להכיל ${t.inclusive ? `${t.minimum} ${e?.unit ?? ""} או יותר` : `יותר מ-${t.minimum} ${e?.unit ?? ""}`}`.trim();
				}
				let r = t.inclusive ? ">=" : ">", s = a(t.origin ?? "value");
				return e?.unit ? `${e.shortLabel} מדי: ${n} ${s} ${r}${t.minimum.toString()} ${e.unit}` : `${e?.shortLabel ?? "קטן"} מדי: ${n} ${s} ${r}${t.minimum.toString()}`;
			}
			case "invalid_format": {
				let e = t;
				if (e.format === "starts_with") return `המחרוזת חייבת להתחיל ב "${e.prefix}"`;
				if (e.format === "ends_with") return `המחרוזת חייבת להסתיים ב "${e.suffix}"`;
				if (e.format === "includes") return `המחרוזת חייבת לכלול "${e.includes}"`;
				if (e.format === "regex") return `המחרוזת חייבת להתאים לתבנית ${e.pattern}`;
				let n = s[e.format];
				return `${n?.label ?? e.format} לא ${(n?.gender ?? "m") === "f" ? "תקינה" : "תקין"}`;
			}
			case "not_multiple_of": return `מספר לא תקין: חייב להיות מכפלה של ${t.divisor}`;
			case "unrecognized_keys": return `מפתח${t.keys.length > 1 ? "ות" : ""} לא מזוה${t.keys.length > 1 ? "ים" : "ה"}: ${u(t.keys, ", ")}`;
			case "invalid_key": return "שדה לא תקין באובייקט";
			case "invalid_union": return "קלט לא תקין";
			case "invalid_element": return `ערך לא תקין ב${i(t.origin ?? "array")}`;
			default: return "קלט לא תקין";
		}
	};
};
function Ds() {
	return { localeError: Es() };
}
// node_modules/zod/v4/locales/hi.js
var Os = () => {
	let e = {
		string: {
			unit: "अक्षर",
			verb: "रखने के लिए"
		},
		file: {
			unit: "बाइट्स",
			verb: "रखने के लिए"
		},
		array: {
			unit: "तत्व",
			verb: "रखने के लिए"
		},
		set: {
			unit: "तत्व",
			verb: "रखने के लिए"
		},
		map: {
			unit: "प्रविष्टियाँ",
			verb: "रखने के लिए"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "इनपुट",
		email: "ईमेल पता",
		url: "URL",
		emoji: "इमोजी",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO तिथि और समय",
		date: "ISO तिथि",
		time: "ISO समय",
		duration: "ISO अवधि",
		ipv4: "IPv4 पता",
		ipv6: "IPv6 पता",
		mac: "MAC पता",
		cidrv4: "IPv4 श्रेणी",
		cidrv6: "IPv6 श्रेणी",
		base64: "Base64-एन्कोडेड स्ट्रिंग",
		base64url: "Base64URL-एन्कोडेड स्ट्रिंग",
		json_string: "JSON स्ट्रिंग",
		e164: "E.164 संख्या",
		credit_card: "क्रेडिट कार्ड संख्या",
		currency_code: "मुद्रा कोड",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "इनपुट"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input);
				return `अमान्य इनपुट: अपेक्षित ${t}, प्राप्त ${r[n] ?? n}`;
			}
			case "invalid_value": return e.values.length === 1 ? `अमान्य इनपुट: अपेक्षित ${S(e.values[0])}` : `अमान्य विकल्प: अपेक्षित मानों में से एक ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `बहुत बड़ा: अपेक्षित था कि ${e.origin ?? "मान"} में ${n}${e.maximum} ${r.unit} हों` : `बहुत बड़ा: अपेक्षित था कि ${e.origin ?? "मान"} ${n}${e.maximum} हो`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `बहुत छोटा: अपेक्षित था कि ${e.origin} में ${n}${e.minimum} ${r.unit} हों` : `बहुत छोटा: अपेक्षित था कि ${e.origin} ${n}${e.minimum} हो`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `अमान्य स्ट्रिंग: "${t.prefix}" से शुरू होना चाहिए` : t.format === "ends_with" ? `अमान्य स्ट्रिंग: "${t.suffix}" पर समाप्त होना चाहिए` : t.format === "includes" ? `अमान्य स्ट्रिंग: इसमें "${t.includes}" शामिल होना चाहिए` : t.format === "regex" ? `अमान्य स्ट्रिंग: पैटर्न ${t.pattern} से मेल खाना चाहिए` : `अमान्य ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `अमान्य संख्या: यह ${e.divisor} का गुणज होना चाहिए`;
			case "unrecognized_keys": return `अपरिचित कुंजी${e.keys.length > 1 ? "याँ" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `अमान्य कुंजी: ${e.origin} में`;
			case "invalid_union": return e.options && Array.isArray(e.options) && e.options.length > 0 ? `अमान्य डिस्क्रिमिनेटर मान: अपेक्षित ${e.options.map((e) => `'${e}'`).join(" | ")}` : "अमान्य इनपुट";
			case "invalid_element": return `अमान्य मान: ${e.origin} में`;
			default: return "अमान्य इनपुट";
		}
	};
};
function ks() {
	return { localeError: Os() };
}
// node_modules/zod/v4/locales/hr.js
var As = () => {
	let e = {
		string: {
			unit: "znakova",
			verb: "imati"
		},
		file: {
			unit: "bajtova",
			verb: "imati"
		},
		array: {
			unit: "stavki",
			verb: "imati"
		},
		set: {
			unit: "stavki",
			verb: "imati"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "unos",
		email: "email adresa",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO datum i vrijeme",
		date: "ISO datum",
		time: "ISO vrijeme",
		duration: "ISO trajanje",
		ipv4: "IPv4 adresa",
		ipv6: "IPv6 adresa",
		mac: "MAC adresa",
		cidrv4: "IPv4 raspon",
		cidrv6: "IPv6 raspon",
		base64: "base64 kodirani tekst",
		base64url: "base64url kodirani tekst",
		json_string: "JSON tekst",
		e164: "E.164 broj",
		credit_card: "broj kreditne kartice",
		currency_code: "kod valute",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "unos"
	}, r = {
		nan: "NaN",
		string: "tekst",
		number: "broj",
		boolean: "boolean",
		array: "niz",
		object: "objekt",
		set: "skup",
		file: "datoteka",
		date: "datum",
		bigint: "bigint",
		symbol: "simbol",
		undefined: "undefined",
		null: "null",
		function: "funkcija",
		map: "mapa"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Neispravan unos: očekuje se instanceof ${e.expected}, a primljeno je ${i}` : `Neispravan unos: očekuje se ${t}, a primljeno je ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Neispravna vrijednost: očekivano ${S(e.values[0])}` : `Neispravna opcija: očekivano jedno od ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", i = t(e.origin), a = r[e.origin] ?? e.origin;
				return i ? `Preveliko: očekivano da ${a ?? "vrijednost"} ima ${n}${e.maximum.toString()} ${i.unit ?? "elemenata"}` : `Preveliko: očekivano da ${a ?? "vrijednost"} bude ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", i = t(e.origin), a = r[e.origin] ?? e.origin;
				return i ? `Premalo: očekivano da ${a} ima ${n}${e.minimum.toString()} ${i.unit}` : `Premalo: očekivano da ${a} bude ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Neispravan tekst: mora započinjati s "${t.prefix}"` : t.format === "ends_with" ? `Neispravan tekst: mora završavati s "${t.suffix}"` : t.format === "includes" ? `Neispravan tekst: mora sadržavati "${t.includes}"` : t.format === "regex" ? `Neispravan tekst: mora odgovarati uzorku ${t.pattern}` : `Neispravna ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Neispravan broj: mora biti višekratnik od ${e.divisor}`;
			case "unrecognized_keys": return `Neprepoznat${e.keys.length > 1 ? "i ključevi" : " ključ"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Neispravan ključ u ${r[e.origin] ?? e.origin}`;
			case "invalid_union": return "Neispravan unos";
			case "invalid_element": return `Neispravna vrijednost u ${r[e.origin] ?? e.origin}`;
			default: return "Neispravan unos";
		}
	};
};
function js() {
	return { localeError: As() };
}
// node_modules/zod/v4/locales/hu.js
var Ms = () => {
	let e = {
		string: {
			unit: "karakter",
			verb: "legyen"
		},
		file: {
			unit: "byte",
			verb: "legyen"
		},
		array: {
			unit: "elem",
			verb: "legyen"
		},
		set: {
			unit: "elem",
			verb: "legyen"
		},
		map: {
			unit: "elem",
			verb: "legyen"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "bemenet",
		email: "email cím",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO időbélyeg",
		date: "ISO dátum",
		time: "ISO idő",
		duration: "ISO időintervallum",
		ipv4: "IPv4 cím",
		ipv6: "IPv6 cím",
		mac: "MAC cím",
		cidrv4: "IPv4 tartomány",
		cidrv6: "IPv6 tartomány",
		base64: "base64-kódolt string",
		base64url: "base64url-kódolt string",
		json_string: "JSON string",
		e164: "E.164 szám",
		credit_card: "hitelkártyaszám",
		currency_code: "pénznemkód",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "bemenet"
	}, r = {
		nan: "NaN",
		number: "szám",
		array: "tömb"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Érvénytelen bemenet: a várt érték instanceof ${e.expected}, a kapott érték ${i}` : `Érvénytelen bemenet: a várt érték ${t}, a kapott érték ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Érvénytelen bemenet: a várt érték ${S(e.values[0])}` : `Érvénytelen opció: valamelyik érték várt ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Túl nagy: ${e.origin ?? "érték"} mérete túl nagy ${n}${e.maximum.toString()} ${r.unit ?? "elem"}` : `Túl nagy: a bemeneti érték ${e.origin ?? "érték"} túl nagy: ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Túl kicsi: a bemeneti érték ${e.origin} mérete túl kicsi ${n}${e.minimum.toString()} ${r.unit}` : `Túl kicsi: a bemeneti érték ${e.origin} túl kicsi ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Érvénytelen string: "${t.prefix}" értékkel kell kezdődnie` : t.format === "ends_with" ? `Érvénytelen string: "${t.suffix}" értékkel kell végződnie` : t.format === "includes" ? `Érvénytelen string: "${t.includes}" értéket kell tartalmaznia` : t.format === "regex" ? `Érvénytelen string: ${t.pattern} mintának kell megfelelnie` : `Érvénytelen ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Érvénytelen szám: ${e.divisor} többszörösének kell lennie`;
			case "unrecognized_keys": return `Ismeretlen kulcs${e.keys.length > 1 ? "s" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Érvénytelen kulcs ${e.origin}`;
			case "invalid_union": return "Érvénytelen bemenet";
			case "invalid_element": return `Érvénytelen érték: ${e.origin}`;
			default: return "Érvénytelen bemenet";
		}
	};
};
function Ns() {
	return { localeError: Ms() };
}
// node_modules/zod/v4/locales/hy.js
function Ps(e, t, n) {
	return Math.abs(e) === 1 ? t : n;
}
function Fs(e) {
	if (!e) return "";
	let t = [
		"ա",
		"ե",
		"ը",
		"ի",
		"ո",
		"ու",
		"օ"
	], n = e[e.length - 1];
	return e + (t.includes(n) ? "ն" : "ը");
}
var Is = () => {
	let e = {
		string: {
			unit: {
				one: "նշան",
				many: "նշաններ"
			},
			verb: "ունենալ"
		},
		file: {
			unit: {
				one: "բայթ",
				many: "բայթեր"
			},
			verb: "ունենալ"
		},
		array: {
			unit: {
				one: "տարր",
				many: "տարրեր"
			},
			verb: "ունենալ"
		},
		set: {
			unit: {
				one: "տարր",
				many: "տարրեր"
			},
			verb: "ունենալ"
		},
		map: {
			unit: {
				one: "տարր",
				many: "տարրեր"
			},
			verb: "ունենալ"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "մուտք",
		email: "էլ. հասցե",
		url: "URL",
		emoji: "էմոջի",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO ամսաթիվ և ժամ",
		date: "ISO ամսաթիվ",
		time: "ISO ժամ",
		duration: "ISO տևողություն",
		ipv4: "IPv4 հասցե",
		ipv6: "IPv6 հասցե",
		mac: "MAC հասցե",
		cidrv4: "IPv4 միջակայք",
		cidrv6: "IPv6 միջակայք",
		base64: "base64 ձևաչափով տող",
		base64url: "base64url ձևաչափով տող",
		json_string: "JSON տող",
		e164: "E.164 համար",
		credit_card: "կրեդիտ քարտի համար",
		currency_code: "արժույթի կոդ",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "մուտք"
	}, r = {
		nan: "NaN",
		number: "թիվ",
		array: "զանգված"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Սխալ մուտքագրում․ սպասվում էր instanceof ${e.expected}, ստացվել է ${i}` : `Սխալ մուտքագրում․ սպասվում էր ${t}, ստացվել է ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Սխալ մուտքագրում․ սպասվում էր ${S(e.values[1])}` : `Սխալ տարբերակ․ սպասվում էր հետևյալներից մեկը՝ ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				if (r) {
					let t = Ps(Number(e.maximum), r.unit.one, r.unit.many);
					return `Չափազանց մեծ արժեք․ սպասվում է, որ ${Fs(e.origin ?? "արժեք")} կունենա ${n}${e.maximum.toString()} ${t}`;
				}
				return `Չափազանց մեծ արժեք․ սպասվում է, որ ${Fs(e.origin ?? "արժեք")} լինի ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				if (r) {
					let t = Ps(Number(e.minimum), r.unit.one, r.unit.many);
					return `Չափազանց փոքր արժեք․ սպասվում է, որ ${Fs(e.origin)} կունենա ${n}${e.minimum.toString()} ${t}`;
				}
				return `Չափազանց փոքր արժեք․ սպասվում է, որ ${Fs(e.origin)} լինի ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Սխալ տող․ պետք է սկսվի "${t.prefix}"-ով` : t.format === "ends_with" ? `Սխալ տող․ պետք է ավարտվի "${t.suffix}"-ով` : t.format === "includes" ? `Սխալ տող․ պետք է պարունակի "${t.includes}"` : t.format === "regex" ? `Սխալ տող․ պետք է համապատասխանի ${t.pattern} ձևաչափին` : `Սխալ ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Սխալ թիվ․ պետք է բազմապատիկ լինի ${e.divisor}-ի`;
			case "unrecognized_keys": return `Չճանաչված բանալի${e.keys.length > 1 ? "ներ" : ""}. ${u(e.keys, ", ")}`;
			case "invalid_key": return `Սխալ բանալի ${Fs(e.origin)}-ում`;
			case "invalid_union": return "Սխալ մուտքագրում";
			case "invalid_element": return `Սխալ արժեք ${Fs(e.origin)}-ում`;
			default: return "Սխալ մուտքագրում";
		}
	};
};
function Ls() {
	return { localeError: Is() };
}
// node_modules/zod/v4/locales/id.js
var Rs = () => {
	let e = {
		string: {
			unit: "karakter",
			verb: "memiliki"
		},
		file: {
			unit: "byte",
			verb: "memiliki"
		},
		array: {
			unit: "item",
			verb: "memiliki"
		},
		set: {
			unit: "item",
			verb: "memiliki"
		},
		map: {
			unit: "item",
			verb: "memiliki"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "input",
		email: "alamat email",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "tanggal dan waktu format ISO",
		date: "tanggal format ISO",
		time: "jam format ISO",
		duration: "durasi format ISO",
		ipv4: "alamat IPv4",
		ipv6: "alamat IPv6",
		mac: "alamat MAC",
		cidrv4: "rentang alamat IPv4",
		cidrv6: "rentang alamat IPv6",
		base64: "string dengan enkode base64",
		base64url: "string dengan enkode base64url",
		json_string: "string JSON",
		e164: "angka E.164",
		credit_card: "nomor kartu kredit",
		currency_code: "kode mata uang",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "input"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Input tidak valid: diharapkan instanceof ${e.expected}, diterima ${i}` : `Input tidak valid: diharapkan ${t}, diterima ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Input tidak valid: diharapkan ${S(e.values[0])}` : `Pilihan tidak valid: diharapkan salah satu dari ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Terlalu besar: diharapkan ${e.origin ?? "value"} memiliki ${n}${e.maximum.toString()} ${r.unit ?? "elemen"}` : `Terlalu besar: diharapkan ${e.origin ?? "value"} menjadi ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Terlalu kecil: diharapkan ${e.origin} memiliki ${n}${e.minimum.toString()} ${r.unit}` : `Terlalu kecil: diharapkan ${e.origin} menjadi ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `String tidak valid: harus dimulai dengan "${t.prefix}"` : t.format === "ends_with" ? `String tidak valid: harus berakhir dengan "${t.suffix}"` : t.format === "includes" ? `String tidak valid: harus menyertakan "${t.includes}"` : t.format === "regex" ? `String tidak valid: harus sesuai pola ${t.pattern}` : `${n[t.format] ?? e.format} tidak valid`;
			}
			case "not_multiple_of": return `Angka tidak valid: harus kelipatan dari ${e.divisor}`;
			case "unrecognized_keys": return `Kunci tidak dikenali ${e.keys.length > 1 ? "s" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Kunci tidak valid di ${e.origin}`;
			case "invalid_union": return "Input tidak valid";
			case "invalid_element": return `Nilai tidak valid di ${e.origin}`;
			default: return "Input tidak valid";
		}
	};
};
function zs() {
	return { localeError: Rs() };
}
// node_modules/zod/v4/locales/is.js
var Bs = () => {
	let e = {
		string: {
			unit: "stafi",
			verb: "að hafa"
		},
		file: {
			unit: "bæti",
			verb: "að hafa"
		},
		array: {
			unit: "hluti",
			verb: "að hafa"
		},
		set: {
			unit: "hluti",
			verb: "að hafa"
		},
		map: {
			unit: "hluti",
			verb: "að hafa"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "gildi",
		email: "netfang",
		url: "vefslóð",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO dagsetning og tími",
		date: "ISO dagsetning",
		time: "ISO tími",
		duration: "ISO tímalengd",
		ipv4: "IPv4 address",
		ipv6: "IPv6 address",
		mac: "MAC address",
		cidrv4: "IPv4 range",
		cidrv6: "IPv6 range",
		base64: "base64-encoded strengur",
		base64url: "base64url-encoded strengur",
		json_string: "JSON strengur",
		e164: "E.164 tölugildi",
		credit_card: "kreditkortanúmer",
		currency_code: "gjaldmiðilskóði",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "gildi"
	}, r = {
		nan: "NaN",
		number: "númer",
		array: "fylki"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Rangt gildi: Þú slóst inn ${i} þar sem á að vera instanceof ${e.expected}` : `Rangt gildi: Þú slóst inn ${i} þar sem á að vera ${t}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Rangt gildi: gert ráð fyrir ${S(e.values[0])}` : `Ógilt val: má vera eitt af eftirfarandi ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Of stórt: gert er ráð fyrir að ${e.origin ?? "gildi"} hafi ${n}${e.maximum.toString()} ${r.unit ?? "hluti"}` : `Of stórt: gert er ráð fyrir að ${e.origin ?? "gildi"} sé ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Of lítið: gert er ráð fyrir að ${e.origin} hafi ${n}${e.minimum.toString()} ${r.unit}` : `Of lítið: gert er ráð fyrir að ${e.origin} sé ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Ógildur strengur: verður að byrja á "${t.prefix}"` : t.format === "ends_with" ? `Ógildur strengur: verður að enda á "${t.suffix}"` : t.format === "includes" ? `Ógildur strengur: verður að innihalda "${t.includes}"` : t.format === "regex" ? `Ógildur strengur: verður að fylgja mynstri ${t.pattern}` : `Rangt ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Röng tala: verður að vera margfeldi af ${e.divisor}`;
			case "unrecognized_keys": return `Óþekkt ${e.keys.length > 1 ? "ir lyklar" : "ur lykill"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Rangur lykill í ${e.origin}`;
			case "invalid_union": return "Rangt gildi";
			case "invalid_element": return `Rangt gildi í ${e.origin}`;
			default: return "Rangt gildi";
		}
	};
};
function Vs() {
	return { localeError: Bs() };
}
// node_modules/zod/v4/locales/it.js
var Hs = () => {
	let e = {
		string: {
			unit: "caratteri",
			verb: "avere"
		},
		file: {
			unit: "byte",
			verb: "avere"
		},
		array: {
			unit: "elementi",
			verb: "avere"
		},
		set: {
			unit: "elementi",
			verb: "avere"
		},
		map: {
			unit: "elementi",
			verb: "avere"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "input",
		email: "indirizzo email",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "data e ora ISO",
		date: "data ISO",
		time: "ora ISO",
		duration: "durata ISO",
		ipv4: "indirizzo IPv4",
		ipv6: "indirizzo IPv6",
		mac: "indirizzo MAC",
		cidrv4: "intervallo IPv4",
		cidrv6: "intervallo IPv6",
		base64: "stringa codificata in base64",
		base64url: "URL codificata in base64",
		json_string: "stringa JSON",
		e164: "numero E.164",
		credit_card: "numero di carta di credito",
		currency_code: "codice valuta",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "input"
	}, r = {
		nan: "NaN",
		number: "numero",
		array: "vettore"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Input non valido: atteso instanceof ${e.expected}, ricevuto ${i}` : `Input non valido: atteso ${t}, ricevuto ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Input non valido: atteso ${S(e.values[0])}` : `Opzione non valida: atteso uno tra ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Troppo grande: ${e.origin ?? "valore"} deve avere ${n}${e.maximum.toString()} ${r.unit ?? "elementi"}` : `Troppo grande: ${e.origin ?? "valore"} deve essere ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Troppo piccolo: ${e.origin} deve avere ${n}${e.minimum.toString()} ${r.unit}` : `Troppo piccolo: ${e.origin} deve essere ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Stringa non valida: deve iniziare con "${t.prefix}"` : t.format === "ends_with" ? `Stringa non valida: deve terminare con "${t.suffix}"` : t.format === "includes" ? `Stringa non valida: deve includere "${t.includes}"` : t.format === "regex" ? `Stringa non valida: deve corrispondere al pattern ${t.pattern}` : `Input non valido: ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Numero non valido: deve essere un multiplo di ${e.divisor}`;
			case "unrecognized_keys": return `Chiav${e.keys.length > 1 ? "i" : "e"} non riconosciut${e.keys.length > 1 ? "e" : "a"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Chiave non valida in ${e.origin}`;
			case "invalid_union": return "Input non valido";
			case "invalid_element": return `Valore non valido in ${e.origin}`;
			default: return "Input non valido";
		}
	};
};
function Us() {
	return { localeError: Hs() };
}
// node_modules/zod/v4/locales/ja.js
var Ws = () => {
	let e = {
		string: {
			unit: "文字",
			verb: "である"
		},
		file: {
			unit: "バイト",
			verb: "である"
		},
		array: {
			unit: "要素",
			verb: "である"
		},
		set: {
			unit: "要素",
			verb: "である"
		},
		map: {
			unit: "要素",
			verb: "である"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "入力値",
		email: "メールアドレス",
		url: "URL",
		emoji: "絵文字",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO日時",
		date: "ISO日付",
		time: "ISO時刻",
		duration: "ISO期間",
		ipv4: "IPv4アドレス",
		ipv6: "IPv6アドレス",
		mac: "MACアドレス",
		cidrv4: "IPv4範囲",
		cidrv6: "IPv6範囲",
		base64: "base64エンコード文字列",
		base64url: "base64urlエンコード文字列",
		json_string: "JSON文字列",
		e164: "E.164番号",
		credit_card: "クレジットカード番号",
		currency_code: "通貨コード",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "入力値"
	}, r = {
		nan: "NaN",
		number: "数値",
		array: "配列"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `無効な入力: instanceof ${e.expected}が期待されましたが、${i}が入力されました` : `無効な入力: ${t}が期待されましたが、${i}が入力されました`;
			}
			case "invalid_value": return e.values.length === 1 ? `無効な入力: ${S(e.values[0])}が期待されました` : `無効な選択: ${u(e.values, "、")}のいずれかである必要があります`;
			case "too_big": {
				let n = e.inclusive ? "以下である" : "より小さい", r = t(e.origin);
				return r ? `大きすぎる値: ${e.origin ?? "値"}は${e.maximum.toString()}${r.unit ?? "要素"}${n}必要があります` : `大きすぎる値: ${e.origin ?? "値"}は${e.maximum.toString()}${n}必要があります`;
			}
			case "too_small": {
				let n = e.inclusive ? "以上である" : "より大きい", r = t(e.origin);
				return r ? `小さすぎる値: ${e.origin}は${e.minimum.toString()}${r.unit}${n}必要があります` : `小さすぎる値: ${e.origin}は${e.minimum.toString()}${n}必要があります`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `無効な文字列: "${t.prefix}"で始まる必要があります` : t.format === "ends_with" ? `無効な文字列: "${t.suffix}"で終わる必要があります` : t.format === "includes" ? `無効な文字列: "${t.includes}"を含む必要があります` : t.format === "regex" ? `無効な文字列: パターン${t.pattern}に一致する必要があります` : `無効な${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `無効な数値: ${e.divisor}の倍数である必要があります`;
			case "unrecognized_keys": return `認識されていないキー${e.keys.length > 1 ? "群" : ""}: ${u(e.keys, "、")}`;
			case "invalid_key": return `${e.origin}内の無効なキー`;
			case "invalid_union": return "無効な入力";
			case "invalid_element": return `${e.origin}内の無効な値`;
			default: return "無効な入力";
		}
	};
};
function Gs() {
	return { localeError: Ws() };
}
// node_modules/zod/v4/locales/ka.js
var Ks = () => {
	let e = {
		string: {
			unit: "სიმბოლო",
			verb: "უნდა შეიცავდეს"
		},
		file: {
			unit: "ბაიტი",
			verb: "უნდა შეიცავდეს"
		},
		array: {
			unit: "ელემენტი",
			verb: "უნდა შეიცავდეს"
		},
		set: {
			unit: "ელემენტი",
			verb: "უნდა შეიცავდეს"
		},
		map: {
			unit: "ელემენტი",
			verb: "უნდა შეიცავდეს"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "შეყვანა",
		email: "ელ-ფოსტის მისამართი",
		url: "URL",
		emoji: "ემოჯი",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "თარიღი-დრო",
		date: "თარიღი",
		time: "დრო",
		duration: "ხანგრძლივობა",
		ipv4: "IPv4 მისამართი",
		ipv6: "IPv6 მისამართი",
		mac: "MAC მისამართი",
		cidrv4: "IPv4 დიაპაზონი",
		cidrv6: "IPv6 დიაპაზონი",
		base64: "base64-კოდირებული ველი",
		base64url: "base64url-კოდირებული ველი",
		json_string: "JSON ველი",
		e164: "E.164 ნომერი",
		credit_card: "საკრედიტო ბარათის ნომერი",
		currency_code: "ვალუტის კოდი",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "შეყვანა"
	}, r = {
		nan: "NaN",
		number: "რიცხვი",
		string: "ველი",
		boolean: "ბულეანი",
		function: "ფუნქცია",
		array: "მასივი"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `არასწორი შეყვანა: მოსალოდნელი instanceof ${e.expected}, მიღებული ${i}` : `არასწორი შეყვანა: მოსალოდნელი ${t}, მიღებული ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `არასწორი შეყვანა: მოსალოდნელი ${S(e.values[0])}` : `არასწორი ვარიანტი: მოსალოდნელია ერთ-ერთი ${u(e.values, "|")}-დან`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `ზედმეტად დიდი: მოსალოდნელი ${e.origin ?? "მნიშვნელობა"} ${r.verb} ${n}${e.maximum.toString()} ${r.unit}` : `ზედმეტად დიდი: მოსალოდნელი ${e.origin ?? "მნიშვნელობა"} იყოს ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `ზედმეტად პატარა: მოსალოდნელი ${e.origin} ${r.verb} ${n}${e.minimum.toString()} ${r.unit}` : `ზედმეტად პატარა: მოსალოდნელი ${e.origin} იყოს ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `არასწორი ველი: უნდა იწყებოდეს "${t.prefix}"-ით` : t.format === "ends_with" ? `არასწორი ველი: უნდა მთავრდებოდეს "${t.suffix}"-ით` : t.format === "includes" ? `არასწორი ველი: უნდა შეიცავდეს "${t.includes}"-ს` : t.format === "regex" ? `არასწორი ველი: უნდა შეესაბამებოდეს შაბლონს ${t.pattern}` : `არასწორი ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `არასწორი რიცხვი: უნდა იყოს ${e.divisor}-ის ჯერადი`;
			case "unrecognized_keys": return `უცნობი გასაღებ${e.keys.length > 1 ? "ები" : "ი"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `არასწორი გასაღები ${e.origin}-ში`;
			case "invalid_union": return "არასწორი შეყვანა";
			case "invalid_element": return `არასწორი მნიშვნელობა ${e.origin}-ში`;
			default: return "არასწორი შეყვანა";
		}
	};
};
function qs() {
	return { localeError: Ks() };
}
// node_modules/zod/v4/locales/km.js
var Js = () => {
	let e = {
		string: {
			unit: "តួអក្សរ",
			verb: "គួរមាន"
		},
		file: {
			unit: "បៃ",
			verb: "គួរមាន"
		},
		array: {
			unit: "ធាតុ",
			verb: "គួរមាន"
		},
		set: {
			unit: "ធាតុ",
			verb: "គួរមាន"
		},
		map: {
			unit: "ធាតុ",
			verb: "គួរមាន"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "ទិន្នន័យបញ្ចូល",
		email: "អាសយដ្ឋានអ៊ីមែល",
		url: "URL",
		emoji: "សញ្ញាអារម្មណ៍",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "កាលបរិច្ឆេទ និងម៉ោង ISO",
		date: "កាលបរិច្ឆេទ ISO",
		time: "ម៉ោង ISO",
		duration: "រយៈពេល ISO",
		ipv4: "អាសយដ្ឋាន IPv4",
		ipv6: "អាសយដ្ឋាន IPv6",
		mac: "អាសយដ្ឋាន MAC",
		cidrv4: "ដែនអាសយដ្ឋាន IPv4",
		cidrv6: "ដែនអាសយដ្ឋាន IPv6",
		base64: "ខ្សែអក្សរអ៊ិកូដ base64",
		base64url: "ខ្សែអក្សរអ៊ិកូដ base64url",
		json_string: "ខ្សែអក្សរ JSON",
		e164: "លេខ E.164",
		credit_card: "លេខប័ណ្ណឥណទាន",
		currency_code: "កូដរូបិយប័ណ្ណ",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "ទិន្នន័យបញ្ចូល"
	}, r = {
		nan: "NaN",
		number: "លេខ",
		array: "អារេ (Array)",
		null: "គ្មានតម្លៃ (null)"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `ទិន្នន័យបញ្ចូលមិនត្រឹមត្រូវ៖ ត្រូវការ instanceof ${e.expected} ប៉ុន្តែទទួលបាន ${i}` : `ទិន្នន័យបញ្ចូលមិនត្រឹមត្រូវ៖ ត្រូវការ ${t} ប៉ុន្តែទទួលបាន ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `ទិន្នន័យបញ្ចូលមិនត្រឹមត្រូវ៖ ត្រូវការ ${S(e.values[0])}` : `ជម្រើសមិនត្រឹមត្រូវ៖ ត្រូវជាមួយក្នុងចំណោម ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `ធំពេក៖ ត្រូវការ ${e.origin ?? "តម្លៃ"} ${n} ${e.maximum.toString()} ${r.unit ?? "ធាតុ"}` : `ធំពេក៖ ត្រូវការ ${e.origin ?? "តម្លៃ"} ${n} ${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `តូចពេក៖ ត្រូវការ ${e.origin} ${n} ${e.minimum.toString()} ${r.unit}` : `តូចពេក៖ ត្រូវការ ${e.origin} ${n} ${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `ខ្សែអក្សរមិនត្រឹមត្រូវ៖ ត្រូវចាប់ផ្តើមដោយ "${t.prefix}"` : t.format === "ends_with" ? `ខ្សែអក្សរមិនត្រឹមត្រូវ៖ ត្រូវបញ្ចប់ដោយ "${t.suffix}"` : t.format === "includes" ? `ខ្សែអក្សរមិនត្រឹមត្រូវ៖ ត្រូវមាន "${t.includes}"` : t.format === "regex" ? `ខ្សែអក្សរមិនត្រឹមត្រូវ៖ ត្រូវតែផ្គូផ្គងនឹងទម្រង់ដែលបានកំណត់ ${t.pattern}` : `មិនត្រឹមត្រូវ៖ ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `លេខមិនត្រឹមត្រូវ៖ ត្រូវតែជាពហុគុណនៃ ${e.divisor}`;
			case "unrecognized_keys": return `រកឃើញសោមិនស្គាល់៖ ${u(e.keys, ", ")}`;
			case "invalid_key": return `សោមិនត្រឹមត្រូវនៅក្នុង ${e.origin}`;
			case "invalid_union": return "ទិន្នន័យមិនត្រឹមត្រូវ";
			case "invalid_element": return `ទិន្នន័យមិនត្រឹមត្រូវនៅក្នុង ${e.origin}`;
			default: return "ទិន្នន័យមិនត្រឹមត្រូវ";
		}
	};
};
function Ys() {
	return { localeError: Js() };
}
// node_modules/zod/v4/locales/kh.js
function Xs() {
	return Ys();
}
// node_modules/zod/v4/locales/kn.js
var Zs = () => {
	let e = {
		string: {
			unit: "ಅಕ್ಷರಗಳು",
			verb: "ಹೊಂದಲು"
		},
		file: {
			unit: "ಬೈಟ್‌ಗಳು",
			verb: "ಹೊಂದಲು"
		},
		array: {
			unit: "ವಸ್ತುಗಳು",
			verb: "ಹೊಂದಲು"
		},
		set: {
			unit: "ವಸ್ತುಗಳು",
			verb: "ಹೊಂದಲು"
		},
		map: {
			unit: "entries",
			verb: "ಹೊಂದಲು"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "ಇನ್ಪುಟ್",
		email: "email ವಿಳಾಸ",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO ದಿನಾಂಕದ ಸಮಯ",
		date: "ISO ದಿನಾಂಕ",
		time: "ISO ಸಮಯ",
		duration: "ISO ಅವಧಿ",
		ipv4: "IPv4 ವಿಳಾಸ",
		ipv6: "IPv6 ವಿಳಾಸ",
		mac: "MAC ವಿಳಾಸ",
		cidrv4: "IPv4 ವ್ಯಾಪ್ತಿಯ",
		cidrv6: "IPv6 ವ್ಯಾಪ್ತಿಯ",
		base64: "base64-encodedಸ್ಟ್ರಿಂಗ್",
		base64url: "base64url-encodedಸ್ಟ್ರಿಂಗ್",
		json_string: "JSONಸ್ಟ್ರಿಂಗ್",
		e164: "E.164 ಸಂಖ್ಯೆ",
		credit_card: "ಕ್ರೆಡಿಟ್ ಕಾರ್ಡ್ ಸಂಖ್ಯೆ",
		currency_code: "ಕರೆನ್ಸಿ ಕೋಡ್",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "ಇನ್ಪುಟ್"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input);
				return `ಅಮಾನ್ಯ ಇನ್‌ಪುಟ್: ನಿರೀಕ್ಷಿಸಲಾಗಿದೆ ${t}, ಸ್ವೀಕರಿಸಿದನು ${r[n] ?? n}`;
			}
			case "invalid_value": return e.values.length === 1 ? `ಅಮಾನ್ಯ ಇನ್‌ಪುಟ್: ನಿರೀಕ್ಷಿಸಲಾಗಿದೆ ${S(e.values[0])}` : `ಅಮಾನ್ಯ ಆಯ್ಕೆ: ಇವುಗಳಲ್ಲಿ ಒಂದನ್ನು ನಿರೀಕ್ಷಿಸಲಾಗಿದೆ ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `ತುಂಬಾ ದೊಡ್ಡದು: ನಿರೀಕ್ಷಿಸಲಾಗಿದೆ ${e.origin ?? "value"} ಹೊಂದಲು ${n}${e.maximum.toString()} ${r.unit ?? "ಅಂಶಗಳು"}` : `ತುಂಬಾ ದೊಡ್ಡದು: ನಿರೀಕ್ಷಿಸಲಾಗಿದೆ ${e.origin ?? "value"} ಎಂದು ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `ತುಂಬಾ ಚಿಕ್ಕದು: ನಿರೀಕ್ಷಿಸಲಾಗಿದೆ ${e.origin} ಹೊಂದಲು ${n}${e.minimum.toString()} ${r.unit}` : `ತುಂಬಾ ಚಿಕ್ಕದು: ನಿರೀಕ್ಷಿಸಲಾಗಿದೆ ${e.origin} ಎಂದು ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `ಅಮಾನ್ಯವಾದ ಸ್ಟ್ರಿಂಗ್: ಇದರೊಂದಿಗೆ ಪ್ರಾರಂಭಿಸಬೇಕು "${t.prefix}"` : t.format === "ends_with" ? `ಅಮಾನ್ಯವಾದ ಸ್ಟ್ರಿಂಗ್: ಇದರೊಂದಿಗೆ ಕೊನೆಗೊಳ್ಳಬೇಕು "${t.suffix}"` : t.format === "includes" ? `ಅಮಾನ್ಯ ಸ್ಟ್ರಿಂಗ್: ಒಳಗೊಂಡಿರಬೇಕು "${t.includes}"` : t.format === "regex" ? `ಅಮಾನ್ಯವಾದ ಸ್ಟ್ರಿಂಗ್: ಮಾದರಿಗೆ ಹೊಂದಿಕೆಯಾಗಬೇಕು ${t.pattern}` : `ಅಮಾನ್ಯ ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `ಅಮಾನ್ಯ ಸಂಖ್ಯೆ: ಬಹುಸಂಖ್ಯೆಯಾಗಿರಬೇಕು ${e.divisor}`;
			case "unrecognized_keys": return `ಗುರುತಿಸಲಾಗದ ಕೀ ${e.keys.length > 1 ? "s" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `ಅಮಾನ್ಯವಾದ ಕೀ ಇನ್ ${e.origin}`;
			case "invalid_union": return e.options && Array.isArray(e.options) && e.options.length > 0 ? `ಅಮಾನ್ಯ ತಾರತಮ್ಯ ಮೌಲ್ಯ. ನಿರೀಕ್ಷಿಸಲಾಗಿದೆ ${e.options.map((e) => `'${e}'`).join(" | ")}` : "ಅಮಾನ್ಯ ಇನ್‌ಪುಟ್";
			case "invalid_element": return `ರಲ್ಲಿ ಅಮಾನ್ಯ ಮೌಲ್ಯ ${e.origin}`;
			default: return "ಅಮಾನ್ಯ ಇನ್‌ಪುಟ್";
		}
	};
};
function Qs() {
	return { localeError: Zs() };
}
// node_modules/zod/v4/locales/ko.js
var $s = () => {
	let e = {
		string: {
			unit: "문자",
			verb: "to have"
		},
		file: {
			unit: "바이트",
			verb: "to have"
		},
		array: {
			unit: "개",
			verb: "to have"
		},
		set: {
			unit: "개",
			verb: "to have"
		},
		map: {
			unit: "개",
			verb: "to have"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "입력",
		email: "이메일 주소",
		url: "URL",
		emoji: "이모지",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO 날짜시간",
		date: "ISO 날짜",
		time: "ISO 시간",
		duration: "ISO 기간",
		ipv4: "IPv4 주소",
		ipv6: "IPv6 주소",
		mac: "MAC 주소",
		cidrv4: "IPv4 범위",
		cidrv6: "IPv6 범위",
		base64: "base64 인코딩 문자열",
		base64url: "base64url 인코딩 문자열",
		json_string: "JSON 문자열",
		e164: "E.164 번호",
		credit_card: "신용카드 번호",
		currency_code: "통화 코드",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "입력"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `잘못된 입력: 예상 타입은 instanceof ${e.expected}, 받은 타입은 ${i}입니다` : `잘못된 입력: 예상 타입은 ${t}, 받은 타입은 ${i}입니다`;
			}
			case "invalid_value": return e.values.length === 1 ? `잘못된 입력: 값은 ${S(e.values[0])} 이어야 합니다` : `잘못된 옵션: ${u(e.values, "또는 ")} 중 하나여야 합니다`;
			case "too_big": {
				let n = e.inclusive ? "이하" : "미만", r = n === "미만" ? "이어야 합니다" : "여야 합니다", i = t(e.origin), a = i?.unit ?? "요소";
				return i ? `${e.origin ?? "값"}이 너무 큽니다: ${e.maximum.toString()}${a} ${n}${r}` : `${e.origin ?? "값"}이 너무 큽니다: ${e.maximum.toString()} ${n}${r}`;
			}
			case "too_small": {
				let n = e.inclusive ? "이상" : "초과", r = n === "이상" ? "이어야 합니다" : "여야 합니다", i = t(e.origin), a = i?.unit ?? "요소";
				return i ? `${e.origin ?? "값"}이 너무 작습니다: ${e.minimum.toString()}${a} ${n}${r}` : `${e.origin ?? "값"}이 너무 작습니다: ${e.minimum.toString()} ${n}${r}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `잘못된 문자열: "${t.prefix}"(으)로 시작해야 합니다` : t.format === "ends_with" ? `잘못된 문자열: "${t.suffix}"(으)로 끝나야 합니다` : t.format === "includes" ? `잘못된 문자열: "${t.includes}"을(를) 포함해야 합니다` : t.format === "regex" ? `잘못된 문자열: 정규식 ${t.pattern} 패턴과 일치해야 합니다` : `잘못된 ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `잘못된 숫자: ${e.divisor}의 배수여야 합니다`;
			case "unrecognized_keys": return `인식할 수 없는 키: ${u(e.keys, ", ")}`;
			case "invalid_key": return `잘못된 키: ${e.origin}`;
			case "invalid_union": return "잘못된 입력";
			case "invalid_element": return `잘못된 값: ${e.origin}`;
			default: return "잘못된 입력";
		}
	};
};
function ec() {
	return { localeError: $s() };
}
// node_modules/zod/v4/locales/lt.js
var tc = (e) => e.charAt(0).toUpperCase() + e.slice(1);
function nc(e) {
	let t = Math.abs(e), n = t % 10, r = t % 100;
	return r >= 11 && r <= 19 || n === 0 ? "many" : n === 1 ? "one" : "few";
}
var rc = () => {
	let e = {
		string: {
			unit: {
				one: "simbolis",
				few: "simboliai",
				many: "simbolių"
			},
			verb: {
				smaller: {
					inclusive: "turi būti ne ilgesnė kaip",
					notInclusive: "turi būti trumpesnė kaip"
				},
				bigger: {
					inclusive: "turi būti ne trumpesnė kaip",
					notInclusive: "turi būti ilgesnė kaip"
				}
			}
		},
		file: {
			unit: {
				one: "baitas",
				few: "baitai",
				many: "baitų"
			},
			verb: {
				smaller: {
					inclusive: "turi būti ne didesnis kaip",
					notInclusive: "turi būti mažesnis kaip"
				},
				bigger: {
					inclusive: "turi būti ne mažesnis kaip",
					notInclusive: "turi būti didesnis kaip"
				}
			}
		},
		array: {
			unit: {
				one: "elementą",
				few: "elementus",
				many: "elementų"
			},
			verb: {
				smaller: {
					inclusive: "turi turėti ne daugiau kaip",
					notInclusive: "turi turėti mažiau kaip"
				},
				bigger: {
					inclusive: "turi turėti ne mažiau kaip",
					notInclusive: "turi turėti daugiau kaip"
				}
			}
		},
		set: {
			unit: {
				one: "elementą",
				few: "elementus",
				many: "elementų"
			},
			verb: {
				smaller: {
					inclusive: "turi turėti ne daugiau kaip",
					notInclusive: "turi turėti mažiau kaip"
				},
				bigger: {
					inclusive: "turi turėti ne mažiau kaip",
					notInclusive: "turi turėti daugiau kaip"
				}
			}
		}
	};
	function t(t, n, r, i) {
		let a = e[t] ?? null;
		return a === null ? a : {
			unit: a.unit[n],
			verb: a.verb[i][r ? "inclusive" : "notInclusive"]
		};
	}
	let n = {
		regex: "įvestis",
		email: "el. pašto adresas",
		url: "URL",
		emoji: "jaustukas",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO data ir laikas",
		date: "ISO data",
		time: "ISO laikas",
		duration: "ISO trukmė",
		ipv4: "IPv4 adresas",
		ipv6: "IPv6 adresas",
		mac: "MAC adresas",
		cidrv4: "IPv4 tinklo prefiksas (CIDR)",
		cidrv6: "IPv6 tinklo prefiksas (CIDR)",
		base64: "base64 užkoduota eilutė",
		base64url: "base64url užkoduota eilutė",
		json_string: "JSON eilutė",
		e164: "E.164 numeris",
		credit_card: "kredito kortelės numeris",
		currency_code: "valiutos kodas",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "įvestis"
	}, r = {
		nan: "NaN",
		number: "skaičius",
		bigint: "sveikasis skaičius",
		string: "eilutė",
		boolean: "loginė reikšmė",
		undefined: "neapibrėžta reikšmė",
		function: "funkcija",
		symbol: "simbolis",
		array: "masyvas",
		object: "objektas",
		null: "nulinė reikšmė"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Gautas tipas ${i}, o tikėtasi - instanceof ${e.expected}` : `Gautas tipas ${i}, o tikėtasi - ${t}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Privalo būti ${S(e.values[0])}` : `Privalo būti vienas iš ${u(e.values, "|")} pasirinkimų`;
			case "too_big": {
				let n = r[e.origin] ?? e.origin, i = t(e.origin, nc(Number(e.maximum)), e.inclusive ?? !1, "smaller");
				if (i?.verb) return `${tc(n ?? e.origin ?? "reikšmė")} ${i.verb} ${e.maximum.toString()} ${i.unit ?? "elementų"}`;
				let a = e.inclusive ? "ne didesnis kaip" : "mažesnis kaip";
				return `${tc(n ?? e.origin ?? "reikšmė")} turi būti ${a} ${e.maximum.toString()} ${i?.unit}`;
			}
			case "too_small": {
				let n = r[e.origin] ?? e.origin, i = t(e.origin, nc(Number(e.minimum)), e.inclusive ?? !1, "bigger");
				if (i?.verb) return `${tc(n ?? e.origin ?? "reikšmė")} ${i.verb} ${e.minimum.toString()} ${i.unit ?? "elementų"}`;
				let a = e.inclusive ? "ne mažesnis kaip" : "didesnis kaip";
				return `${tc(n ?? e.origin ?? "reikšmė")} turi būti ${a} ${e.minimum.toString()} ${i?.unit}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Eilutė privalo prasidėti "${t.prefix}"` : t.format === "ends_with" ? `Eilutė privalo pasibaigti "${t.suffix}"` : t.format === "includes" ? `Eilutė privalo įtraukti "${t.includes}"` : t.format === "regex" ? `Eilutė privalo atitikti ${t.pattern}` : `Neteisingas ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Skaičius privalo būti ${e.divisor} kartotinis.`;
			case "unrecognized_keys": return `Neatpažint${e.keys.length > 1 ? "i" : "as"} rakt${e.keys.length > 1 ? "ai" : "as"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return "Rastas klaidingas raktas";
			case "invalid_union": return "Klaidinga įvestis";
			case "invalid_element": return `${tc(r[e.origin] ?? e.origin ?? e.origin ?? "reikšmė")} turi klaidingą įvestį`;
			default: return "Klaidinga įvestis";
		}
	};
};
function ic() {
	return { localeError: rc() };
}
// node_modules/zod/v4/locales/mk.js
var ac = () => {
	let e = {
		string: {
			unit: "знаци",
			verb: "да имаат"
		},
		file: {
			unit: "бајти",
			verb: "да имаат"
		},
		array: {
			unit: "ставки",
			verb: "да имаат"
		},
		set: {
			unit: "ставки",
			verb: "да имаат"
		},
		map: {
			unit: "ставки",
			verb: "да имаат"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "внес",
		email: "адреса на е-пошта",
		url: "URL",
		emoji: "емоџи",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO датум и време",
		date: "ISO датум",
		time: "ISO време",
		duration: "ISO времетраење",
		ipv4: "IPv4 адреса",
		ipv6: "IPv6 адреса",
		mac: "MAC адреса",
		cidrv4: "IPv4 опсег",
		cidrv6: "IPv6 опсег",
		base64: "base64-енкодирана низа",
		base64url: "base64url-енкодирана низа",
		json_string: "JSON низа",
		e164: "E.164 број",
		credit_card: "број на кредитна картичка",
		currency_code: "код на валута",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "внес"
	}, r = {
		nan: "NaN",
		number: "број",
		array: "низа"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Грешен внес: се очекува instanceof ${e.expected}, примено ${i}` : `Грешен внес: се очекува ${t}, примено ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Invalid input: expected ${S(e.values[0])}` : `Грешана опција: се очекува една ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Премногу голем: се очекува ${e.origin ?? "вредноста"} да има ${n}${e.maximum.toString()} ${r.unit ?? "елементи"}` : `Премногу голем: се очекува ${e.origin ?? "вредноста"} да биде ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Премногу мал: се очекува ${e.origin} да има ${n}${e.minimum.toString()} ${r.unit}` : `Премногу мал: се очекува ${e.origin} да биде ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Неважечка низа: мора да започнува со "${t.prefix}"` : t.format === "ends_with" ? `Неважечка низа: мора да завршува со "${t.suffix}"` : t.format === "includes" ? `Неважечка низа: мора да вклучува "${t.includes}"` : t.format === "regex" ? `Неважечка низа: мора да одгоара на патернот ${t.pattern}` : `Invalid ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Грешен број: мора да биде делив со ${e.divisor}`;
			case "unrecognized_keys": return `${e.keys.length > 1 ? "Непрепознаени клучеви" : "Непрепознаен клуч"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Грешен клуч во ${e.origin}`;
			case "invalid_union": return "Грешен внес";
			case "invalid_element": return `Грешна вредност во ${e.origin}`;
			default: return "Грешен внес";
		}
	};
};
function oc() {
	return { localeError: ac() };
}
// node_modules/zod/v4/locales/ms.js
var sc = () => {
	let e = {
		string: {
			unit: "aksara",
			verb: "mempunyai"
		},
		file: {
			unit: "bait",
			verb: "mempunyai"
		},
		array: {
			unit: "elemen",
			verb: "mempunyai"
		},
		set: {
			unit: "elemen",
			verb: "mempunyai"
		},
		map: {
			unit: "elemen",
			verb: "mempunyai"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "input",
		email: "alamat e-mel",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "tarikh masa ISO",
		date: "tarikh ISO",
		time: "masa ISO",
		duration: "tempoh ISO",
		ipv4: "alamat IPv4",
		ipv6: "alamat IPv6",
		mac: "alamat MAC",
		cidrv4: "julat IPv4",
		cidrv6: "julat IPv6",
		base64: "string dikodkan base64",
		base64url: "string dikodkan base64url",
		json_string: "string JSON",
		e164: "nombor E.164",
		credit_card: "nombor kad kredit",
		currency_code: "kod mata wang",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "input"
	}, r = {
		nan: "NaN",
		number: "nombor"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Input tidak sah: dijangka instanceof ${e.expected}, diterima ${i}` : `Input tidak sah: dijangka ${t}, diterima ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Input tidak sah: dijangka ${S(e.values[0])}` : `Pilihan tidak sah: dijangka salah satu daripada ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Terlalu besar: dijangka ${e.origin ?? "nilai"} ${r.verb} ${n}${e.maximum.toString()} ${r.unit ?? "elemen"}` : `Terlalu besar: dijangka ${e.origin ?? "nilai"} adalah ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Terlalu kecil: dijangka ${e.origin} ${r.verb} ${n}${e.minimum.toString()} ${r.unit}` : `Terlalu kecil: dijangka ${e.origin} adalah ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `String tidak sah: mesti bermula dengan "${t.prefix}"` : t.format === "ends_with" ? `String tidak sah: mesti berakhir dengan "${t.suffix}"` : t.format === "includes" ? `String tidak sah: mesti mengandungi "${t.includes}"` : t.format === "regex" ? `String tidak sah: mesti sepadan dengan corak ${t.pattern}` : `${n[t.format] ?? e.format} tidak sah`;
			}
			case "not_multiple_of": return `Nombor tidak sah: perlu gandaan ${e.divisor}`;
			case "unrecognized_keys": return `Kunci tidak dikenali: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Kunci tidak sah dalam ${e.origin}`;
			case "invalid_union": return "Input tidak sah";
			case "invalid_element": return `Nilai tidak sah dalam ${e.origin}`;
			default: return "Input tidak sah";
		}
	};
};
function cc() {
	return { localeError: sc() };
}
// node_modules/zod/v4/locales/ne.js
var lc = () => {
	let e = {
		string: {
			unit: "अक्षर",
			verb: "हुनुपर्छ"
		},
		file: {
			unit: "बाइट",
			verb: "हुनुपर्छ"
		},
		array: {
			unit: "तत्व",
			verb: "हुनुपर्छ"
		},
		set: {
			unit: "तत्व",
			verb: "हुनुपर्छ"
		},
		map: {
			unit: "प्रविष्टि",
			verb: "हुनुपर्छ"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "इनपुट",
		email: "इमेल ठेगाना",
		url: "URL",
		emoji: "इमोजी",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO मिति र समय",
		date: "ISO मिति",
		time: "ISO समय",
		duration: "ISO अवधि",
		ipv4: "IPv4 ठेगाना",
		ipv6: "IPv6 ठेगाना",
		mac: "MAC ठेगाना",
		cidrv4: "IPv4 दायरा",
		cidrv6: "IPv6 दायरा",
		base64: "base64-इन्कोड गरिएको स्ट्रिङ",
		base64url: "base64url-इन्कोड गरिएको स्ट्रिङ",
		json_string: "JSON स्ट्रिङ",
		e164: "E.164 नम्बर",
		credit_card: "क्रेडिट कार्ड नम्बर",
		currency_code: "मुद्रा कोड",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "इनपुट"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input);
				return `अमान्य इनपुट: अपेक्षित ${t}, प्राप्त ${r[n] ?? n}`;
			}
			case "invalid_value": return e.values.length === 1 ? `अमान्य इनपुट: अपेक्षित ${S(e.values[0])}` : `अमान्य विकल्प: अपेक्षित मानहरू मध्ये एक ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `धेरै ठूलो: ${e.origin ?? "मान"} मा ${n}${e.maximum.toString()} ${r.unit} ${r.verb}` : `धेरै ठूलो: ${e.origin ?? "मान"} ${n}${e.maximum.toString()} हुनुपर्छ`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `धेरै सानो: ${e.origin} मा ${n}${e.minimum.toString()} ${r.unit} ${r.verb}` : `धेरै सानो: ${e.origin} ${n}${e.minimum.toString()} हुनुपर्छ`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `अमान्य स्ट्रिङ: "${t.prefix}" बाट सुरु हुनुपर्छ` : t.format === "ends_with" ? `अमान्य स्ट्रिङ: "${t.suffix}" मा समाप्त हुनुपर्छ` : t.format === "includes" ? `अमान्य स्ट्रिङ: "${t.includes}" समावेश हुनुपर्छ` : t.format === "regex" ? `अमान्य स्ट्रिङ: ढाँचा ${t.pattern} सँग मेल खानुपर्छ` : `अमान्य ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `अमान्य संख्या: ${e.divisor} को गुणज हुनुपर्छ`;
			case "unrecognized_keys": return `अपरिचित कुञ्जी${e.keys.length > 1 ? "हरू" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `अमान्य कुञ्जी: ${e.origin} मा`;
			case "invalid_union": return e.options && Array.isArray(e.options) && e.options.length > 0 ? `अमान्य डिस्क्रिमिनेटर मान: अपेक्षित ${e.options.map((e) => `'${e}'`).join(" | ")}` : "अमान्य इनपुट";
			case "invalid_element": return `अमान्य मान: ${e.origin} मा`;
			default: return "अमान्य इनपुट";
		}
	};
};
function uc() {
	return { localeError: lc() };
}
// node_modules/zod/v4/locales/nl.js
var dc = () => {
	let e = {
		string: {
			unit: "tekens",
			verb: "heeft"
		},
		file: {
			unit: "bytes",
			verb: "heeft"
		},
		array: {
			unit: "elementen",
			verb: "heeft"
		},
		set: {
			unit: "elementen",
			verb: "heeft"
		},
		map: {
			unit: "elementen",
			verb: "heeft"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "invoer",
		email: "emailadres",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO datum en tijd",
		date: "ISO datum",
		time: "ISO tijd",
		duration: "ISO duur",
		ipv4: "IPv4-adres",
		ipv6: "IPv6-adres",
		mac: "MAC-adres",
		cidrv4: "IPv4-bereik",
		cidrv6: "IPv6-bereik",
		base64: "base64-gecodeerde tekst",
		base64url: "base64 URL-gecodeerde tekst",
		json_string: "JSON string",
		e164: "E.164-nummer",
		credit_card: "creditcardnummer",
		currency_code: "valutacode",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "invoer"
	}, r = {
		nan: "NaN",
		number: "getal"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Ongeldige invoer: verwacht instanceof ${e.expected}, ontving ${i}` : `Ongeldige invoer: verwacht ${t}, ontving ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Ongeldige invoer: verwacht ${S(e.values[0])}` : `Ongeldige optie: verwacht één van ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin), i = e.origin === "date" ? "laat" : e.origin === "string" ? "lang" : "groot";
				return r ? `Te ${i}: verwacht dat ${e.origin ?? "waarde"} ${n}${e.maximum.toString()} ${r.unit ?? "elementen"} ${r.verb}` : `Te ${i}: verwacht dat ${e.origin ?? "waarde"} ${n}${e.maximum.toString()} is`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin), i = e.origin === "date" ? "vroeg" : e.origin === "string" ? "kort" : "klein";
				return r ? `Te ${i}: verwacht dat ${e.origin} ${n}${e.minimum.toString()} ${r.unit} ${r.verb}` : `Te ${i}: verwacht dat ${e.origin} ${n}${e.minimum.toString()} is`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Ongeldige tekst: moet met "${t.prefix}" beginnen` : t.format === "ends_with" ? `Ongeldige tekst: moet op "${t.suffix}" eindigen` : t.format === "includes" ? `Ongeldige tekst: moet "${t.includes}" bevatten` : t.format === "regex" ? `Ongeldige tekst: moet overeenkomen met patroon ${t.pattern}` : `Ongeldig: ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Ongeldig getal: moet een veelvoud van ${e.divisor} zijn`;
			case "unrecognized_keys": return `Onbekende key${e.keys.length > 1 ? "s" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Ongeldige key in ${e.origin}`;
			case "invalid_union": return "Ongeldige invoer";
			case "invalid_element": return `Ongeldige waarde in ${e.origin}`;
			default: return "Ongeldige invoer";
		}
	};
};
function fc() {
	return { localeError: dc() };
}
// node_modules/zod/v4/locales/nn.js
var pc = () => {
	let e = {
		string: {
			unit: "teikn",
			verb: "å ha"
		},
		file: {
			unit: "bytes",
			verb: "å ha"
		},
		array: {
			unit: "element",
			verb: "å innehalde"
		},
		set: {
			unit: "element",
			verb: "å innehalde"
		},
		map: {
			unit: "element",
			verb: "å innehalde"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "input",
		email: "e-postadresse",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO dato- og klokkeslett",
		date: "ISO-dato",
		time: "ISO-klokkeslett",
		duration: "ISO-varigheit",
		ipv4: "IPv4-adresse",
		ipv6: "IPv6-adresse",
		mac: "MAC-adresse",
		cidrv4: "IPv4-spekter",
		cidrv6: "IPv6-spekter",
		base64: "base64-enkoda streng",
		base64url: "base64url-enkoda streng",
		json_string: "JSON-streng",
		e164: "E.164-nummer",
		credit_card: "kredittkortnummer",
		currency_code: "valutakode",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "input"
	}, r = {
		nan: "NaN",
		number: "tal",
		array: "liste"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Ugyldig input: forventa instanceof ${e.expected}, fekk ${i}` : `Ugyldig input: forventa ${t}, fekk ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Ugyldig verdi: forventa ${S(e.values[0])}` : `Ugyldig val: forventa eitt av ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `For stor(t): forventa ${e.origin ?? "value"} til å ha ${n}${e.maximum.toString()} ${r.unit ?? "element"}` : `For stor(t): forventa ${e.origin ?? "value"} til å ha ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `For lite(n): forventa ${e.origin} til å ha ${n}${e.minimum.toString()} ${r.unit}` : `For lite(n): forventa ${e.origin} til å ha ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Ugyldig streng: må starte med "${t.prefix}"` : t.format === "ends_with" ? `Ugyldig streng: må slutte med "${t.suffix}"` : t.format === "includes" ? `Ugyldig streng: må innehalde "${t.includes}"` : t.format === "regex" ? `Ugyldig streng: må matche mønsteret ${t.pattern}` : `Ugyldig ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Ugyldig tal: må vere eit multiplum av ${e.divisor}`;
			case "unrecognized_keys": return `${e.keys.length > 1 ? "Ukjende nøklar" : "Ukjend nøkkel"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Ugyldig nøkkel i ${e.origin}`;
			case "invalid_union": return "Ugyldig input";
			case "invalid_element": return `Ugyldig verdi i ${e.origin}`;
			default: return "Ugyldig input";
		}
	};
};
function mc() {
	return { localeError: pc() };
}
// node_modules/zod/v4/locales/no.js
var hc = () => {
	let e = {
		string: {
			unit: "tegn",
			verb: "å ha"
		},
		file: {
			unit: "bytes",
			verb: "å ha"
		},
		array: {
			unit: "elementer",
			verb: "å inneholde"
		},
		set: {
			unit: "elementer",
			verb: "å inneholde"
		},
		map: {
			unit: "elementer",
			verb: "å inneholde"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "input",
		email: "e-postadresse",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO dato- og klokkeslett",
		date: "ISO-dato",
		time: "ISO-klokkeslett",
		duration: "ISO-varighet",
		ipv4: "IPv4-adresse",
		ipv6: "IPv6-adresse",
		mac: "MAC-adresse",
		cidrv4: "IPv4-spekter",
		cidrv6: "IPv6-spekter",
		base64: "base64-enkodet streng",
		base64url: "base64url-enkodet streng",
		json_string: "JSON-streng",
		e164: "E.164-nummer",
		credit_card: "kredittkortnummer",
		currency_code: "valutakode",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "input"
	}, r = {
		nan: "NaN",
		number: "tall",
		array: "liste"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Ugyldig input: forventet instanceof ${e.expected}, fikk ${i}` : `Ugyldig input: forventet ${t}, fikk ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Ugyldig verdi: forventet ${S(e.values[0])}` : `Ugyldig valg: forventet en av ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `For stor(t): forventet ${e.origin ?? "value"} til å ha ${n}${e.maximum.toString()} ${r.unit ?? "elementer"}` : `For stor(t): forventet ${e.origin ?? "value"} til å ha ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `For lite(n): forventet ${e.origin} til å ha ${n}${e.minimum.toString()} ${r.unit}` : `For lite(n): forventet ${e.origin} til å ha ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Ugyldig streng: må starte med "${t.prefix}"` : t.format === "ends_with" ? `Ugyldig streng: må ende med "${t.suffix}"` : t.format === "includes" ? `Ugyldig streng: må inneholde "${t.includes}"` : t.format === "regex" ? `Ugyldig streng: må matche mønsteret ${t.pattern}` : `Ugyldig ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Ugyldig tall: må være et multiplum av ${e.divisor}`;
			case "unrecognized_keys": return `${e.keys.length > 1 ? "Ukjente nøkler" : "Ukjent nøkkel"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Ugyldig nøkkel i ${e.origin}`;
			case "invalid_union": return "Ugyldig input";
			case "invalid_element": return `Ugyldig verdi i ${e.origin}`;
			default: return "Ugyldig input";
		}
	};
};
function gc() {
	return { localeError: hc() };
}
// node_modules/zod/v4/locales/ota.js
var _c = () => {
	let e = {
		string: {
			unit: "harf",
			verb: "olmalıdır"
		},
		file: {
			unit: "bayt",
			verb: "olmalıdır"
		},
		array: {
			unit: "unsur",
			verb: "olmalıdır"
		},
		set: {
			unit: "unsur",
			verb: "olmalıdır"
		},
		map: {
			unit: "unsur",
			verb: "olmalıdır"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "giren",
		email: "epostagâh",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO hengâmı",
		date: "ISO tarihi",
		time: "ISO zamanı",
		duration: "ISO müddeti",
		ipv4: "IPv4 nişânı",
		ipv6: "IPv6 nişânı",
		mac: "MAC nişânı",
		cidrv4: "IPv4 menzili",
		cidrv6: "IPv6 menzili",
		base64: "base64-şifreli metin",
		base64url: "base64url-şifreli metin",
		json_string: "JSON metin",
		e164: "E.164 sayısı",
		credit_card: "i'tibâr kartı numarası",
		currency_code: "para birimi kodu",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "giren"
	}, r = {
		nan: "NaN",
		number: "numara",
		array: "saf",
		null: "gayb"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Fâsit giren: umulan instanceof ${e.expected}, alınan ${i}` : `Fâsit giren: umulan ${t}, alınan ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Fâsit giren: umulan ${S(e.values[0])}` : `Fâsit tercih: mûteberler ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Fazla büyük: ${e.origin ?? "value"}, ${n}${e.maximum.toString()} ${r.unit ?? "elements"} sahip olmalıydı.` : `Fazla büyük: ${e.origin ?? "value"}, ${n}${e.maximum.toString()} olmalıydı.`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Fazla küçük: ${e.origin}, ${n}${e.minimum.toString()} ${r.unit} sahip olmalıydı.` : `Fazla küçük: ${e.origin}, ${n}${e.minimum.toString()} olmalıydı.`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Fâsit metin: "${t.prefix}" ile başlamalı.` : t.format === "ends_with" ? `Fâsit metin: "${t.suffix}" ile bitmeli.` : t.format === "includes" ? `Fâsit metin: "${t.includes}" ihtivâ etmeli.` : t.format === "regex" ? `Fâsit metin: ${t.pattern} nakşına uymalı.` : `Fâsit ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Fâsit sayı: ${e.divisor} katı olmalıydı.`;
			case "unrecognized_keys": return `Tanınmayan anahtar ${e.keys.length > 1 ? "s" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `${e.origin} için tanınmayan anahtar var.`;
			case "invalid_union": return "Giren tanınamadı.";
			case "invalid_element": return `${e.origin} için tanınmayan kıymet var.`;
			default: return "Kıymet tanınamadı.";
		}
	};
};
function vc() {
	return { localeError: _c() };
}
// node_modules/zod/v4/locales/ps.js
var yc = () => {
	let e = {
		string: {
			unit: "توکي",
			verb: "ولري"
		},
		file: {
			unit: "بایټس",
			verb: "ولري"
		},
		array: {
			unit: "توکي",
			verb: "ولري"
		},
		set: {
			unit: "توکي",
			verb: "ولري"
		},
		map: {
			unit: "توکي",
			verb: "ولري"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "ورودي",
		email: "بریښنالیک",
		url: "یو آر ال",
		emoji: "ایموجي",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "نیټه او وخت",
		date: "نېټه",
		time: "وخت",
		duration: "موده",
		ipv4: "د IPv4 پته",
		ipv6: "د IPv6 پته",
		mac: "د MAC پته",
		cidrv4: "د IPv4 ساحه",
		cidrv6: "د IPv6 ساحه",
		base64: "base64-encoded متن",
		base64url: "base64url-encoded متن",
		json_string: "JSON متن",
		e164: "د E.164 شمېره",
		credit_card: "د کریډیټ کارت شمیره",
		currency_code: "د اسعارو کوډ",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "ورودي"
	}, r = {
		nan: "NaN",
		number: "عدد",
		array: "ارې"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `ناسم ورودي: باید instanceof ${e.expected} وای, مګر ${i} ترلاسه شو` : `ناسم ورودي: باید ${t} وای, مګر ${i} ترلاسه شو`;
			}
			case "invalid_value": return e.values.length === 1 ? `ناسم ورودي: باید ${S(e.values[0])} وای` : `ناسم انتخاب: باید یو له ${u(e.values, "|")} څخه وای`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `ډیر لوی: ${e.origin ?? "ارزښت"} باید ${n}${e.maximum.toString()} ${r.unit ?? "عنصرونه"} ولري` : `ډیر لوی: ${e.origin ?? "ارزښت"} باید ${n}${e.maximum.toString()} وي`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `ډیر کوچنی: ${e.origin} باید ${n}${e.minimum.toString()} ${r.unit} ولري` : `ډیر کوچنی: ${e.origin} باید ${n}${e.minimum.toString()} وي`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `ناسم متن: باید د "${t.prefix}" سره پیل شي` : t.format === "ends_with" ? `ناسم متن: باید د "${t.suffix}" سره پای ته ورسيږي` : t.format === "includes" ? `ناسم متن: باید "${t.includes}" ولري` : t.format === "regex" ? `ناسم متن: باید د ${t.pattern} سره مطابقت ولري` : `${n[t.format] ?? e.format} ناسم دی`;
			}
			case "not_multiple_of": return `ناسم عدد: باید د ${e.divisor} مضرب وي`;
			case "unrecognized_keys": return `ناسم ${e.keys.length > 1 ? "کلیډونه" : "کلیډ"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `ناسم کلیډ په ${e.origin} کې`;
			case "invalid_union": return "ناسمه ورودي";
			case "invalid_element": return `ناسم عنصر په ${e.origin} کې`;
			default: return "ناسمه ورودي";
		}
	};
};
function bc() {
	return { localeError: yc() };
}
// node_modules/zod/v4/locales/pl.js
var xc = () => {
	let e = {
		string: {
			unit: "znaków",
			verb: "mieć"
		},
		file: {
			unit: "bajtów",
			verb: "mieć"
		},
		array: {
			unit: "elementów",
			verb: "mieć"
		},
		set: {
			unit: "elementów",
			verb: "mieć"
		},
		map: {
			unit: "elementów",
			verb: "mieć"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "wyrażenie",
		email: "adres email",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "data i godzina w formacie ISO",
		date: "data w formacie ISO",
		time: "godzina w formacie ISO",
		duration: "czas trwania ISO",
		ipv4: "adres IPv4",
		ipv6: "adres IPv6",
		mac: "adres MAC",
		cidrv4: "zakres IPv4",
		cidrv6: "zakres IPv6",
		base64: "ciąg znaków zakodowany w formacie base64",
		base64url: "ciąg znaków zakodowany w formacie base64url",
		json_string: "ciąg znaków w formacie JSON",
		e164: "liczba E.164",
		credit_card: "numer karty kredytowej",
		currency_code: "kod waluty",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "wejście"
	}, r = {
		nan: "NaN",
		number: "liczba",
		array: "tablica"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Nieprawidłowe dane wejściowe: oczekiwano instanceof ${e.expected}, otrzymano ${i}` : `Nieprawidłowe dane wejściowe: oczekiwano ${t}, otrzymano ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Nieprawidłowe dane wejściowe: oczekiwano ${S(e.values[0])}` : `Nieprawidłowa opcja: oczekiwano jednej z wartości ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Za duża wartość: oczekiwano, że ${e.origin ?? "wartość"} będzie mieć ${n}${e.maximum.toString()} ${r.unit ?? "elementów"}` : `Zbyt duż(y/a/e): oczekiwano, że ${e.origin ?? "wartość"} będzie wynosić ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Za mała wartość: oczekiwano, że ${e.origin ?? "wartość"} będzie mieć ${n}${e.minimum.toString()} ${r.unit ?? "elementów"}` : `Zbyt mał(y/a/e): oczekiwano, że ${e.origin ?? "wartość"} będzie wynosić ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Nieprawidłowy ciąg znaków: musi zaczynać się od "${t.prefix}"` : t.format === "ends_with" ? `Nieprawidłowy ciąg znaków: musi kończyć się na "${t.suffix}"` : t.format === "includes" ? `Nieprawidłowy ciąg znaków: musi zawierać "${t.includes}"` : t.format === "regex" ? `Nieprawidłowy ciąg znaków: musi odpowiadać wzorcowi ${t.pattern}` : `Nieprawidłow(y/a/e) ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Nieprawidłowa liczba: musi być wielokrotnością ${e.divisor}`;
			case "unrecognized_keys": return `Nierozpoznane klucze${e.keys.length > 1 ? "s" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Nieprawidłowy klucz w ${e.origin}`;
			case "invalid_union": return "Nieprawidłowe dane wejściowe";
			case "invalid_element": return `Nieprawidłowa wartość w ${e.origin}`;
			default: return "Nieprawidłowe dane wejściowe";
		}
	};
};
function Sc() {
	return { localeError: xc() };
}
// node_modules/zod/v4/locales/pt.js
var Cc = () => {
	let e = {
		string: { unit: "caracteres" },
		file: { unit: "bytes" },
		array: { unit: "elementos" },
		set: { unit: "elementos" },
		map: { unit: "entradas" }
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "a entrada",
		email: "o endereço de e-mail",
		url: "o URL",
		emoji: "o emoji",
		uuid: "o UUID",
		uuidv4: "o UUIDv4",
		uuidv6: "o UUIDv6",
		nanoid: "o nanoid",
		guid: "o GUID",
		cuid: "o cuid",
		cuid2: "o cuid2",
		ulid: "o ULID",
		xid: "o XID",
		ksuid: "o KSUID",
		datetime: "a data e hora ISO",
		date: "a data ISO",
		time: "a hora ISO",
		duration: "a duração ISO",
		ipv4: "o endereço IPv4",
		ipv6: "o endereço IPv6",
		mac: "o endereço MAC",
		cidrv4: "o intervalo de endereços IPv4",
		cidrv6: "o intervalo de endereços IPv6",
		base64: "o texto codificado em base64",
		base64url: "o texto codificado em base64url",
		json_string: "o texto JSON",
		e164: "o número E.164",
		credit_card: "o número de cartão de crédito",
		currency_code: "o código de moeda",
		iban: "o IBAN",
		jwt: "o JWT",
		template_literal: "a entrada"
	}, r = {
		masculine: {
			definite: "o",
			indefinite: "um"
		},
		feminine: {
			definite: "a",
			indefinite: "uma"
		}
	}, i = {
		string: {
			name: "texto",
			articles: r.masculine
		},
		number: {
			name: "número",
			articles: r.masculine
		},
		int: {
			name: "número inteiro",
			articles: r.masculine
		},
		boolean: {
			name: "valor booleano",
			articles: r.masculine
		},
		bigint: {
			name: "número bigint",
			articles: r.masculine
		},
		symbol: {
			name: "símbolo",
			articles: r.masculine
		},
		undefined: {
			name: "valor \"undefined\"",
			articles: r.masculine
		},
		null: {
			name: "valor \"nulo\"",
			articles: r.masculine
		},
		never: {
			name: "valor \"never\"",
			articles: r.masculine
		},
		void: {
			name: "valor \"void\"",
			articles: r.masculine
		},
		date: {
			name: "data",
			articles: r.feminine
		},
		array: {
			name: "vetor",
			articles: r.masculine
		},
		object: {
			name: "objeto",
			articles: r.masculine
		},
		tuple: {
			name: "tuplo",
			articles: r.masculine
		},
		record: {
			name: "registo",
			articles: r.masculine
		},
		map: {
			name: "mapa",
			articles: r.masculine
		},
		set: {
			name: "conjunto",
			articles: r.masculine
		},
		file: {
			name: "ficheiro",
			articles: r.masculine
		},
		nonoptional: {
			name: "valor não opcional",
			articles: r.masculine
		},
		nan: {
			name: "valor \"NaN\"",
			articles: r.masculine
		},
		function: {
			name: "função",
			articles: r.feminine
		}
	};
	function a(e, t) {
		let n = i[e] ?? {
			name: `valor "${e}"`,
			articles: r.masculine
		};
		return `${n.articles[t]} ${n.name}`;
	}
	return (e) => {
		switch (e.code) {
			case "invalid_type": return `Entrada inválida: esperava ${a(e.expected, "indefinite")}, recebeu ${a(E(e.input), "indefinite")}`;
			case "invalid_value": return e.values.length === 1 ? `Entrada inválida: esperava ${S(e.values[0])}` : `Opção inválida: esperava uma das seguintes opções: ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Demasiado grande: esperava que ${a(e.origin, "definite")} tivesse ${n} ${e.maximum.toString()} ${r.unit ?? "elementos"}` : `Demasiado grande: esperava que ${a(e.origin, "definite")} fosse ${n} ${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Demasiado pequeno: esperava que ${a(e.origin, "definite")} tivesse ${n} ${e.minimum.toString()} ${r.unit ?? "elementos"}` : `Demasiado pequeno: esperava que ${a(e.origin, "definite")} fosse ${n} ${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Texto inválido: deve começar por "${t.prefix}"` : t.format === "ends_with" ? `Texto inválido: deve terminar em "${t.suffix}"` : t.format === "includes" ? `Texto inválido: deve incluir "${t.includes}"` : t.format === "regex" ? `Texto inválido: deve corresponder ao padrão ${t.pattern}` : `Formato d${n[t.format] ?? e.format} inválido`;
			}
			case "not_multiple_of": return `Número inválido: deve ser múltiplo de ${e.divisor}`;
			case "unrecognized_keys": {
				let t = e.keys.length > 1 ? "s" : "";
				return `Chave${t} inválida${t}: ${u(e.keys, ", ")}`;
			}
			case "invalid_key": return `Entrada inválida n${a(e.origin, "definite")}`;
			case "invalid_union": return e.options && Array.isArray(e.options) && e.options.length > 0 ? `Valor de discriminação inválido. Esperava ${e.options.map((e) => `'${e}'`).join(" | ")}` : "Entrada inválida";
			case "invalid_element": return `Entrada inválida n${a(e.origin, "definite")}`;
			default: return "Entrada inválida";
		}
	};
};
function wc() {
	return { localeError: Cc() };
}
// node_modules/zod/v4/locales/pt-BR.js
var Tc = () => {
	let e = {
		string: { unit: "caracteres" },
		file: { unit: "bytes" },
		array: { unit: "elementos" },
		set: { unit: "elementos" },
		map: { unit: "entradas" }
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "a entrada",
		email: "o endereço de e-mail",
		url: "o URL",
		emoji: "o emoji",
		uuid: "o UUID",
		uuidv4: "o UUIDv4",
		uuidv6: "o UUIDv6",
		nanoid: "o nanoid",
		guid: "o GUID",
		cuid: "o cuid",
		cuid2: "o cuid2",
		ulid: "o ULID",
		xid: "o XID",
		ksuid: "o KSUID",
		datetime: "a data e hora ISO",
		date: "a data ISO",
		time: "a hora ISO",
		duration: "a duração ISO",
		ipv4: "o endereço IPv4",
		ipv6: "o endereço IPv6",
		mac: "o endereço MAC",
		cidrv4: "a faixa de endereços IPv4",
		cidrv6: "a faixa de endereços IPv6",
		base64: "o texto codificado em base64",
		base64url: "o texto codificado em base64url",
		json_string: "o texto JSON",
		e164: "o número E.164",
		credit_card: "o número de cartão de crédito",
		currency_code: "o código de moeda",
		iban: "o IBAN",
		jwt: "o JWT",
		template_literal: "a entrada"
	}, r = {
		masculine: {
			definite: "o",
			indefinite: "um"
		},
		feminine: {
			definite: "a",
			indefinite: "uma"
		}
	}, i = {
		string: {
			name: "texto",
			articles: r.masculine
		},
		number: {
			name: "número",
			articles: r.masculine
		},
		int: {
			name: "número inteiro",
			articles: r.masculine
		},
		boolean: {
			name: "valor booleano",
			articles: r.masculine
		},
		bigint: {
			name: "número bigint",
			articles: r.masculine
		},
		symbol: {
			name: "símbolo",
			articles: r.masculine
		},
		undefined: {
			name: "valor \"undefined\"",
			articles: r.masculine
		},
		null: {
			name: "valor \"nulo\"",
			articles: r.masculine
		},
		never: {
			name: "valor \"never\"",
			articles: r.masculine
		},
		void: {
			name: "valor \"void\"",
			articles: r.masculine
		},
		date: {
			name: "data",
			articles: r.feminine
		},
		array: {
			name: "vetor",
			articles: r.masculine
		},
		object: {
			name: "objeto",
			articles: r.masculine
		},
		tuple: {
			name: "tupla",
			articles: r.feminine
		},
		record: {
			name: "registro",
			articles: r.masculine
		},
		map: {
			name: "mapa",
			articles: r.masculine
		},
		set: {
			name: "conjunto",
			articles: r.masculine
		},
		file: {
			name: "arquivo",
			articles: r.masculine
		},
		nonoptional: {
			name: "valor não opcional",
			articles: r.masculine
		},
		nan: {
			name: "valor \"NaN\"",
			articles: r.masculine
		},
		function: {
			name: "função",
			articles: r.feminine
		}
	};
	function a(e, t) {
		let n = i[e] ?? {
			name: `valor "${e}"`,
			articles: r.masculine
		};
		return `${n.articles[t]} ${n.name}`;
	}
	return (e) => {
		switch (e.code) {
			case "invalid_type": return `Entrada inválida: esperava ${a(e.expected, "indefinite")}, recebeu ${a(E(e.input), "indefinite")}`;
			case "invalid_value": return e.values.length === 1 ? `Entrada inválida: esperava ${S(e.values[0])}` : `Opção inválida: esperava uma das seguintes opções: ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Grande demais: esperava que ${a(e.origin, "definite")} tivesse ${n} ${e.maximum.toString()} ${r.unit ?? "elementos"}` : `Grande demais: esperava que ${a(e.origin, "definite")} fosse ${n} ${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Pequeno demais: esperava que ${a(e.origin, "definite")} tivesse ${n} ${e.minimum.toString()} ${r.unit ?? "elementos"}` : `Pequeno demais: esperava que ${a(e.origin, "definite")} fosse ${n} ${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Texto inválido: deve começar com "${t.prefix}"` : t.format === "ends_with" ? `Texto inválido: deve terminar com "${t.suffix}"` : t.format === "includes" ? `Texto inválido: deve incluir "${t.includes}"` : t.format === "regex" ? `Texto inválido: deve corresponder ao padrão ${t.pattern}` : `Formato d${n[t.format] ?? e.format} inválido`;
			}
			case "not_multiple_of": return `Número inválido: deve ser múltiplo de ${e.divisor}`;
			case "unrecognized_keys": {
				let t = e.keys.length > 1 ? "s" : "";
				return `Chave${t} inválida${t}: ${u(e.keys, ", ")}`;
			}
			case "invalid_key": return `Entrada inválida n${a(e.origin, "definite")}`;
			case "invalid_union": return e.options && Array.isArray(e.options) && e.options.length > 0 ? `Valor de discriminação inválido. Esperava ${e.options.map((e) => `'${e}'`).join(" | ")}` : "Entrada inválida";
			case "invalid_element": return `Entrada inválida n${a(e.origin, "definite")}`;
			default: return "Entrada inválida";
		}
	};
};
function Ec() {
	return { localeError: Tc() };
}
// node_modules/zod/v4/locales/ro.js
var Dc = () => {
	let e = {
		string: {
			unit: "caractere",
			verb: "să aibă"
		},
		file: {
			unit: "octeți",
			verb: "să aibă"
		},
		array: {
			unit: "elemente",
			verb: "să aibă"
		},
		set: {
			unit: "elemente",
			verb: "să aibă"
		},
		map: {
			unit: "intrări",
			verb: "să aibă"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "intrare",
		email: "adresă de email",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "dată și oră ISO",
		date: "dată ISO",
		time: "oră ISO",
		duration: "durată ISO",
		ipv4: "adresă IPv4",
		ipv6: "adresă IPv6",
		mac: "adresă MAC",
		cidrv4: "interval IPv4",
		cidrv6: "interval IPv6",
		base64: "șir codat base64",
		base64url: "șir codat base64url",
		json_string: "șir JSON",
		e164: "număr E.164",
		credit_card: "număr de card de credit",
		currency_code: "cod valutar",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "intrare"
	}, r = {
		nan: "NaN",
		string: "șir",
		number: "număr",
		boolean: "boolean",
		function: "funcție",
		array: "matrice",
		object: "obiect",
		undefined: "nedefinit",
		symbol: "simbol",
		bigint: "număr mare",
		void: "void",
		never: "never",
		map: "hartă",
		set: "set"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input);
				return `Intrare invalidă: așteptat ${t}, primit ${r[n] ?? n}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Intrare invalidă: așteptat ${S(e.values[0])}` : `Opțiune invalidă: așteptat una dintre ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Prea mare: așteptat ca ${e.origin ?? "valoarea"} ${r.verb} ${n}${e.maximum.toString()} ${r.unit ?? "elemente"}` : `Prea mare: așteptat ca ${e.origin ?? "valoarea"} să fie ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Prea mic: așteptat ca ${e.origin} ${r.verb} ${n}${e.minimum.toString()} ${r.unit}` : `Prea mic: așteptat ca ${e.origin} să fie ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Șir invalid: trebuie să înceapă cu "${t.prefix}"` : t.format === "ends_with" ? `Șir invalid: trebuie să se termine cu "${t.suffix}"` : t.format === "includes" ? `Șir invalid: trebuie să includă "${t.includes}"` : t.format === "regex" ? `Șir invalid: trebuie să se potrivească cu modelul ${t.pattern}` : `Format invalid: ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Număr invalid: trebuie să fie multiplu de ${e.divisor}`;
			case "unrecognized_keys": return `Chei nerecunoscute: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Cheie invalidă în ${e.origin}`;
			case "invalid_union": return "Intrare invalidă";
			case "invalid_element": return `Valoare invalidă în ${e.origin}`;
			default: return "Intrare invalidă";
		}
	};
};
function Oc() {
	return { localeError: Dc() };
}
// node_modules/zod/v4/locales/ru.js
function kc(e, t, n, r) {
	let i = Math.abs(e), a = i % 10, o = i % 100;
	return o >= 11 && o <= 19 ? r : a === 1 ? t : a >= 2 && a <= 4 ? n : r;
}
var Ac = () => {
	let e = {
		string: {
			unit: {
				one: "символ",
				few: "символа",
				many: "символов"
			},
			verb: "иметь"
		},
		file: {
			unit: {
				one: "байт",
				few: "байта",
				many: "байт"
			},
			verb: "иметь"
		},
		array: {
			unit: {
				one: "элемент",
				few: "элемента",
				many: "элементов"
			},
			verb: "иметь"
		},
		set: {
			unit: {
				one: "элемент",
				few: "элемента",
				many: "элементов"
			},
			verb: "иметь"
		},
		map: {
			unit: {
				one: "элемент",
				few: "элемента",
				many: "элементов"
			},
			verb: "иметь"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "ввод",
		email: "email адрес",
		url: "URL",
		emoji: "эмодзи",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO дата и время",
		date: "ISO дата",
		time: "ISO время",
		duration: "ISO длительность",
		ipv4: "IPv4 адрес",
		ipv6: "IPv6 адрес",
		mac: "MAC адрес",
		cidrv4: "IPv4 диапазон",
		cidrv6: "IPv6 диапазон",
		base64: "строка в формате base64",
		base64url: "строка в формате base64url",
		json_string: "JSON строка",
		e164: "номер E.164",
		credit_card: "номер кредитной карты",
		currency_code: "код валюты",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "ввод"
	}, r = {
		nan: "NaN",
		number: "число",
		array: "массив"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Неверный ввод: ожидалось instanceof ${e.expected}, получено ${i}` : `Неверный ввод: ожидалось ${t}, получено ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Неверный ввод: ожидалось ${S(e.values[0])}` : `Неверный вариант: ожидалось одно из ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				if (r) {
					let t = kc(Number(e.maximum), r.unit.one, r.unit.few, r.unit.many);
					return `Слишком большое значение: ожидалось, что ${e.origin ?? "значение"} будет иметь ${n}${e.maximum.toString()} ${t}`;
				}
				return `Слишком большое значение: ожидалось, что ${e.origin ?? "значение"} будет ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				if (r) {
					let t = kc(Number(e.minimum), r.unit.one, r.unit.few, r.unit.many);
					return `Слишком маленькое значение: ожидалось, что ${e.origin} будет иметь ${n}${e.minimum.toString()} ${t}`;
				}
				return `Слишком маленькое значение: ожидалось, что ${e.origin} будет ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Неверная строка: должна начинаться с "${t.prefix}"` : t.format === "ends_with" ? `Неверная строка: должна заканчиваться на "${t.suffix}"` : t.format === "includes" ? `Неверная строка: должна содержать "${t.includes}"` : t.format === "regex" ? `Неверная строка: должна соответствовать шаблону ${t.pattern}` : `Неверный ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Неверное число: должно быть кратным ${e.divisor}`;
			case "unrecognized_keys": return `Нераспознанн${e.keys.length > 1 ? "ые" : "ый"} ключ${e.keys.length > 1 ? "и" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Неверный ключ в ${e.origin}`;
			case "invalid_union": return "Неверные входные данные";
			case "invalid_element": return `Неверное значение в ${e.origin}`;
			default: return "Неверные входные данные";
		}
	};
};
function jc() {
	return { localeError: Ac() };
}
// node_modules/zod/v4/locales/sk.js
var Mc = () => {
	let e = {
		string: {
			unit: "znakov",
			verb: "mať"
		},
		file: {
			unit: "bajtov",
			verb: "mať"
		},
		array: {
			unit: "prvkov",
			verb: "mať"
		},
		set: {
			unit: "prvkov",
			verb: "mať"
		},
		map: {
			unit: "položiek",
			verb: "mať"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "regulárny výraz",
		email: "e-mailová adresa",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "dátum a čas vo formáte ISO",
		date: "dátum vo formáte ISO",
		time: "čas vo formáte ISO",
		duration: "doba trvania ISO",
		ipv4: "IPv4 adresa",
		ipv6: "IPv6 adresa",
		mac: "MAC adresa",
		cidrv4: "rozsah IPv4",
		cidrv6: "rozsah IPv6",
		base64: "reťazec zakódovaný vo formáte base64",
		base64url: "reťazec zakódovaný vo formáte base64url",
		json_string: "reťazec vo formáte JSON",
		e164: "číslo E.164",
		credit_card: "číslo kreditnej karty",
		currency_code: "kód meny",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "vstup"
	}, r = {
		nan: "NaN",
		number: "číslo",
		string: "reťazec",
		function: "funkcia",
		array: "pole"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Neplatný vstup: očakávané instanceof ${e.expected}, obdržané ${i}` : `Neplatný vstup: očakávané ${t}, obdržané ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Neplatný vstup: očakávané ${S(e.values[0])}` : `Neplatný vstup: očakávaná jedna z hodnôt ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Hodnota je príliš veľká: ${e.origin ?? "hodnota"} musí mať ${n}${e.maximum.toString()} ${r.unit ?? "prvkov"}` : `Hodnota je príliš veľká: ${e.origin ?? "hodnota"} musí byť ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Hodnota je príliš malá: ${e.origin ?? "hodnota"} musí mať ${n}${e.minimum.toString()} ${r.unit ?? "prvkov"}` : `Hodnota je príliš malá: ${e.origin ?? "hodnota"} musí byť ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Neplatný reťazec: musí začínať na "${t.prefix}"` : t.format === "ends_with" ? `Neplatný reťazec: musí končiť na "${t.suffix}"` : t.format === "includes" ? `Neplatný reťazec: musí obsahovať "${t.includes}"` : t.format === "regex" ? `Neplatný reťazec: musí zodpovedať vzoru ${t.pattern}` : `Neplatný formát ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Neplatné číslo: musí byť násobkom ${e.divisor}`;
			case "unrecognized_keys": return `Neznáme klúče: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Neplatný klúč v ${e.origin}`;
			case "invalid_union": return "Neplatný vstup";
			case "invalid_element": return `Neplatná hodnota v ${e.origin}`;
			default: return "Neplatný vstup";
		}
	};
};
function Nc() {
	return { localeError: Mc() };
}
// node_modules/zod/v4/locales/sl.js
var Pc = () => {
	let e = {
		string: {
			unit: "znakov",
			verb: "imeti"
		},
		file: {
			unit: "bajtov",
			verb: "imeti"
		},
		array: {
			unit: "elementov",
			verb: "imeti"
		},
		set: {
			unit: "elementov",
			verb: "imeti"
		},
		map: {
			unit: "elementov",
			verb: "imeti"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "vnos",
		email: "e-poštni naslov",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO datum in čas",
		date: "ISO datum",
		time: "ISO čas",
		duration: "ISO trajanje",
		ipv4: "IPv4 naslov",
		ipv6: "IPv6 naslov",
		mac: "MAC naslov",
		cidrv4: "obseg IPv4",
		cidrv6: "obseg IPv6",
		base64: "base64 kodiran niz",
		base64url: "base64url kodiran niz",
		json_string: "JSON niz",
		e164: "E.164 številka",
		credit_card: "številka kreditne kartice",
		currency_code: "koda valute",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "vnos"
	}, r = {
		nan: "NaN",
		number: "število",
		array: "tabela"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Neveljaven vnos: pričakovano instanceof ${e.expected}, prejeto ${i}` : `Neveljaven vnos: pričakovano ${t}, prejeto ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Neveljaven vnos: pričakovano ${S(e.values[0])}` : `Neveljavna možnost: pričakovano eno izmed ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Preveliko: pričakovano, da bo ${e.origin ?? "vrednost"} imelo ${n}${e.maximum.toString()} ${r.unit ?? "elementov"}` : `Preveliko: pričakovano, da bo ${e.origin ?? "vrednost"} ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Premajhno: pričakovano, da bo ${e.origin} imelo ${n}${e.minimum.toString()} ${r.unit}` : `Premajhno: pričakovano, da bo ${e.origin} ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Neveljaven niz: mora se začeti z "${t.prefix}"` : t.format === "ends_with" ? `Neveljaven niz: mora se končati z "${t.suffix}"` : t.format === "includes" ? `Neveljaven niz: mora vsebovati "${t.includes}"` : t.format === "regex" ? `Neveljaven niz: mora ustrezati vzorcu ${t.pattern}` : `Neveljaven ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Neveljavno število: mora biti večkratnik ${e.divisor}`;
			case "unrecognized_keys": return `Neprepoznan${e.keys.length > 1 ? "i ključi" : " ključ"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Neveljaven ključ v ${e.origin}`;
			case "invalid_union": return "Neveljaven vnos";
			case "invalid_element": return `Neveljavna vrednost v ${e.origin}`;
			default: return "Neveljaven vnos";
		}
	};
};
function Fc() {
	return { localeError: Pc() };
}
// node_modules/zod/v4/locales/sv.js
var Ic = () => {
	let e = {
		string: {
			unit: "tecken",
			verb: "att ha"
		},
		file: {
			unit: "bytes",
			verb: "att ha"
		},
		array: {
			unit: "objekt",
			verb: "att innehålla"
		},
		set: {
			unit: "objekt",
			verb: "att innehålla"
		},
		map: {
			unit: "objekt",
			verb: "att innehålla"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "reguljärt uttryck",
		email: "e-postadress",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO-datum och tid",
		date: "ISO-datum",
		time: "ISO-tid",
		duration: "ISO-varaktighet",
		ipv4: "IPv4-adress",
		ipv6: "IPv6-adress",
		mac: "MAC-adress",
		cidrv4: "IPv4-spektrum",
		cidrv6: "IPv6-spektrum",
		base64: "base64-kodad sträng",
		base64url: "base64url-kodad sträng",
		json_string: "JSON-sträng",
		e164: "E.164-nummer",
		credit_card: "kreditkortsnummer",
		currency_code: "valutakod",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "mall-literal"
	}, r = {
		nan: "NaN",
		number: "antal",
		array: "lista"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Ogiltig inmatning: förväntat instanceof ${e.expected}, fick ${i}` : `Ogiltig inmatning: förväntat ${t}, fick ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Ogiltig inmatning: förväntat ${S(e.values[0])}` : `Ogiltigt val: förväntade en av ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `För stor(t): förväntade ${e.origin ?? "värdet"} att ha ${n}${e.maximum.toString()} ${r.unit ?? "element"}` : `För stor(t): förväntat ${e.origin ?? "värdet"} att ha ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `För lite(t): förväntade ${e.origin ?? "värdet"} att ha ${n}${e.minimum.toString()} ${r.unit}` : `För lite(t): förväntade ${e.origin ?? "värdet"} att ha ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Ogiltig sträng: måste börja med "${t.prefix}"` : t.format === "ends_with" ? `Ogiltig sträng: måste sluta med "${t.suffix}"` : t.format === "includes" ? `Ogiltig sträng: måste innehålla "${t.includes}"` : t.format === "regex" ? `Ogiltig sträng: måste matcha mönstret "${t.pattern}"` : `Ogiltig(t) ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Ogiltigt tal: måste vara en multipel av ${e.divisor}`;
			case "unrecognized_keys": return `${e.keys.length > 1 ? "Okända nycklar" : "Okänd nyckel"}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Ogiltig nyckel i ${e.origin ?? "värdet"}`;
			case "invalid_union": return "Ogiltig input";
			case "invalid_element": return `Ogiltigt värde i ${e.origin ?? "värdet"}`;
			default: return "Ogiltig input";
		}
	};
};
function Lc() {
	return { localeError: Ic() };
}
// node_modules/zod/v4/locales/ta.js
var Rc = () => {
	let e = {
		string: {
			unit: "எழுத்துக்கள்",
			verb: "கொண்டிருக்க வேண்டும்"
		},
		file: {
			unit: "பைட்டுகள்",
			verb: "கொண்டிருக்க வேண்டும்"
		},
		array: {
			unit: "உறுப்புகள்",
			verb: "கொண்டிருக்க வேண்டும்"
		},
		set: {
			unit: "உறுப்புகள்",
			verb: "கொண்டிருக்க வேண்டும்"
		},
		map: {
			unit: "உறுப்புகள்",
			verb: "கொண்டிருக்க வேண்டும்"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "உள்ளீடு",
		email: "மின்னஞ்சல் முகவரி",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO தேதி நேரம்",
		date: "ISO தேதி",
		time: "ISO நேரம்",
		duration: "ISO கால அளவு",
		ipv4: "IPv4 முகவரி",
		ipv6: "IPv6 முகவரி",
		mac: "MAC முகவரி",
		cidrv4: "IPv4 வரம்பு",
		cidrv6: "IPv6 வரம்பு",
		base64: "base64-encoded சரம்",
		base64url: "base64url-encoded சரம்",
		json_string: "JSON சரம்",
		e164: "E.164 எண்",
		credit_card: "கடன் அட்டை எண்",
		currency_code: "நாணயக் குறியீடு",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "input"
	}, r = {
		nan: "NaN",
		number: "எண்",
		array: "அணி",
		null: "வெறுமை"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `தவறான உள்ளீடு: எதிர்பார்க்கப்பட்டது instanceof ${e.expected}, பெறப்பட்டது ${i}` : `தவறான உள்ளீடு: எதிர்பார்க்கப்பட்டது ${t}, பெறப்பட்டது ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `தவறான உள்ளீடு: எதிர்பார்க்கப்பட்டது ${S(e.values[0])}` : `தவறான விருப்பம்: எதிர்பார்க்கப்பட்டது ${u(e.values, "|")} இல் ஒன்று`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `மிக பெரியது: எதிர்பார்க்கப்பட்டது ${e.origin ?? "மதிப்பு"} ${n}${e.maximum.toString()} ${r.unit ?? "உறுப்புகள்"} ஆக இருக்க வேண்டும்` : `மிக பெரியது: எதிர்பார்க்கப்பட்டது ${e.origin ?? "மதிப்பு"} ${n}${e.maximum.toString()} ஆக இருக்க வேண்டும்`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `மிகச் சிறியது: எதிர்பார்க்கப்பட்டது ${e.origin} ${n}${e.minimum.toString()} ${r.unit} ஆக இருக்க வேண்டும்` : `மிகச் சிறியது: எதிர்பார்க்கப்பட்டது ${e.origin} ${n}${e.minimum.toString()} ஆக இருக்க வேண்டும்`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `தவறான சரம்: "${t.prefix}" இல் தொடங்க வேண்டும்` : t.format === "ends_with" ? `தவறான சரம்: "${t.suffix}" இல் முடிவடைய வேண்டும்` : t.format === "includes" ? `தவறான சரம்: "${t.includes}" ஐ உள்ளடக்க வேண்டும்` : t.format === "regex" ? `தவறான சரம்: ${t.pattern} முறைபாட்டுடன் பொருந்த வேண்டும்` : `தவறான ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `தவறான எண்: ${e.divisor} இன் பலமாக இருக்க வேண்டும்`;
			case "unrecognized_keys": return `அடையாளம் தெரியாத விசை${e.keys.length > 1 ? "கள்" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `${e.origin} இல் தவறான விசை`;
			case "invalid_union": return "தவறான உள்ளீடு";
			case "invalid_element": return `${e.origin} இல் தவறான மதிப்பு`;
			default: return "தவறான உள்ளீடு";
		}
	};
};
function zc() {
	return { localeError: Rc() };
}
// node_modules/zod/v4/locales/tg.js
var Bc = () => {
	let e = {
		string: {
			unit: "аломат",
			verb: "дошта бошад"
		},
		file: {
			unit: "байт",
			verb: "дошта бошад"
		},
		array: {
			unit: "унсур",
			verb: "дошта бошад"
		},
		set: {
			unit: "унсур",
			verb: "дошта бошад"
		},
		map: {
			unit: "сабт",
			verb: "дошта бошад"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "вуруд",
		email: "суроғаи email",
		url: "URL",
		emoji: "эмоҷи",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "санаву вақти ISO",
		date: "санаи ISO",
		time: "вақти ISO",
		duration: "давомнокии ISO",
		ipv4: "суроғаи IPv4",
		ipv6: "суроғаи IPv6",
		mac: "суроғаи MAC",
		cidrv4: "маҳдудаи IPv4",
		cidrv6: "маҳдудаи IPv6",
		base64: "сатри дар формати base64",
		base64url: "сатри дар формати base64url",
		json_string: "сатри JSON",
		e164: "рақами E.164",
		credit_card: "рақами корти кредитӣ",
		currency_code: "рамзи асъор",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "вуруд"
	}, r = {
		nan: "NaN",
		number: "рақам",
		string: "сатр",
		array: "массив",
		object: "объект",
		date: "сана"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input);
				return `Вуруди нодуруст: ${t} интизор мерафт, ${r[n] ?? n} гирифта шуд`;
			}
			case "invalid_value": return e.values.length === 1 ? `Вуруди нодуруст: ${S(e.values[0])} интизор мерафт` : `Интихоби нодуруст: яке аз ${u(e.values, "|")} интизор мерафт`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Хеле калон: ${e.origin ?? "қимат"} бояд ${n}${e.maximum.toString()} ${r.unit} ${r.verb}` : `Хеле калон: ${e.origin ?? "қимат"} бояд ${n}${e.maximum.toString()} бошад`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Хеле хурд: ${e.origin} бояд ${n}${e.minimum.toString()} ${r.unit} ${r.verb}` : `Хеле хурд: ${e.origin} бояд ${n}${e.minimum.toString()} бошад`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Сатри нодуруст: бояд бо "${t.prefix}" оғоз шавад` : t.format === "ends_with" ? `Сатри нодуруст: бояд бо "${t.suffix}" анҷом ёбад` : t.format === "includes" ? `Сатри нодуруст: бояд "${t.includes}"-ро дар бар гирад` : t.format === "regex" ? `Сатри нодуруст: бояд ба намунаи ${t.pattern} мувофиқат кунад` : `${n[t.format] ?? e.format}-и нодуруст`;
			}
			case "not_multiple_of": return `Рақами нодуруст: бояд ба ${e.divisor} бе бақия тақсим шавад`;
			case "unrecognized_keys": return `Калид${e.keys.length > 1 ? "ҳои" : "и"} номаълум: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Калиди нодуруст дар ${e.origin}`;
			case "invalid_union": return e.options && Array.isArray(e.options) && e.options.length > 0 ? `Қимати нодурусти дискриминатор: ${e.options.map((e) => `'${e}'`).join(" | ")} интизор мерафт` : "Вуруди нодуруст";
			case "invalid_element": return `Қимати нодуруст дар ${e.origin}`;
			default: return "Вуруди нодуруст";
		}
	};
};
function Vc() {
	return { localeError: Bc() };
}
// node_modules/zod/v4/locales/th.js
var Hc = () => {
	let e = {
		string: {
			unit: "ตัวอักษร",
			verb: "ควรมี"
		},
		file: {
			unit: "ไบต์",
			verb: "ควรมี"
		},
		array: {
			unit: "รายการ",
			verb: "ควรมี"
		},
		set: {
			unit: "รายการ",
			verb: "ควรมี"
		},
		map: {
			unit: "รายการ",
			verb: "ควรมี"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "ข้อมูลที่ป้อน",
		email: "ที่อยู่อีเมล",
		url: "URL",
		emoji: "อิโมจิ",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "วันที่เวลาแบบ ISO",
		date: "วันที่แบบ ISO",
		time: "เวลาแบบ ISO",
		duration: "ช่วงเวลาแบบ ISO",
		ipv4: "ที่อยู่ IPv4",
		ipv6: "ที่อยู่ IPv6",
		mac: "ที่อยู่ MAC",
		cidrv4: "ช่วง IP แบบ IPv4",
		cidrv6: "ช่วง IP แบบ IPv6",
		base64: "ข้อความแบบ Base64",
		base64url: "ข้อความแบบ Base64 สำหรับ URL",
		json_string: "ข้อความแบบ JSON",
		e164: "เบอร์โทรศัพท์ระหว่างประเทศ (E.164)",
		credit_card: "หมายเลขบัตรเครดิต",
		currency_code: "รหัสสกุลเงิน",
		iban: "IBAN",
		jwt: "โทเคน JWT",
		template_literal: "ข้อมูลที่ป้อน"
	}, r = {
		nan: "NaN",
		number: "ตัวเลข",
		array: "อาร์เรย์ (Array)",
		null: "ไม่มีค่า (null)"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `ประเภทข้อมูลไม่ถูกต้อง: ควรเป็น instanceof ${e.expected} แต่ได้รับ ${i}` : `ประเภทข้อมูลไม่ถูกต้อง: ควรเป็น ${t} แต่ได้รับ ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `ค่าไม่ถูกต้อง: ควรเป็น ${S(e.values[0])}` : `ตัวเลือกไม่ถูกต้อง: ควรเป็นหนึ่งใน ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "ไม่เกิน" : "น้อยกว่า", r = t(e.origin);
				return r ? `เกินกำหนด: ${e.origin ?? "ค่า"} ควรมี${n} ${e.maximum.toString()} ${r.unit ?? "รายการ"}` : `เกินกำหนด: ${e.origin ?? "ค่า"} ควรมี${n} ${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? "อย่างน้อย" : "มากกว่า", r = t(e.origin);
				return r ? `น้อยกว่ากำหนด: ${e.origin} ควรมี${n} ${e.minimum.toString()} ${r.unit}` : `น้อยกว่ากำหนด: ${e.origin} ควรมี${n} ${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `รูปแบบไม่ถูกต้อง: ข้อความต้องขึ้นต้นด้วย "${t.prefix}"` : t.format === "ends_with" ? `รูปแบบไม่ถูกต้อง: ข้อความต้องลงท้ายด้วย "${t.suffix}"` : t.format === "includes" ? `รูปแบบไม่ถูกต้อง: ข้อความต้องมี "${t.includes}" อยู่ในข้อความ` : t.format === "regex" ? `รูปแบบไม่ถูกต้อง: ต้องตรงกับรูปแบบที่กำหนด ${t.pattern}` : `รูปแบบไม่ถูกต้อง: ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `ตัวเลขไม่ถูกต้อง: ต้องเป็นจำนวนที่หารด้วย ${e.divisor} ได้ลงตัว`;
			case "unrecognized_keys": return `พบคีย์ที่ไม่รู้จัก: ${u(e.keys, ", ")}`;
			case "invalid_key": return `คีย์ไม่ถูกต้องใน ${e.origin}`;
			case "invalid_union": return "ข้อมูลไม่ถูกต้อง: ไม่ตรงกับรูปแบบยูเนียนที่กำหนดไว้";
			case "invalid_element": return `ข้อมูลไม่ถูกต้องใน ${e.origin}`;
			default: return "ข้อมูลไม่ถูกต้อง";
		}
	};
};
function Uc() {
	return { localeError: Hc() };
}
// node_modules/zod/v4/locales/tk.js
var Wc = () => {
	let e = {
		string: {
			unit: "simwol",
			verb: "bolmaly"
		},
		file: {
			unit: "baýt",
			verb: "bolmaly"
		},
		array: {
			unit: "elementler",
			verb: "bolmaly"
		},
		set: {
			unit: "elementler",
			verb: "bolmaly"
		},
		map: {
			unit: "elementler",
			verb: "bolmaly"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "giriş",
		email: "e-poçta salgysy",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO sene we wagt",
		date: "ISO sene",
		time: "ISO wagt",
		duration: "ISO wagt aralygy",
		ipv4: "IPv4 salgysy",
		ipv6: "IPv6 salgysy",
		mac: "MAC salgysy",
		cidrv4: "IPv4 aralygy",
		cidrv6: "IPv6 aralygy",
		base64: "base64 bilen şifrlenen setir",
		base64url: "base64url bilen şifrlenen setir",
		json_string: "JSON setiri",
		e164: "E.164 nomeri",
		credit_card: "kredit kartynyň nomeri",
		currency_code: "walýuta kody",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "şablon"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input);
				return `Nädogry baha: garaşylan ${t} ýerine ${r[n] ?? n} alyndy`;
			}
			case "invalid_value": return e.values.length === 1 ? `Nädogry baha: ${S(e.values[0])} bolmaly` : `Nädogry saýlaw: aşakdakylardan biri bolmaly: ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Has uly: garaşylýan ${e.origin ?? "baha"} ${n} ${e.maximum.toString()} ${r.unit ?? "element"}` : `Has uly: garaşylýan ${e.origin ?? "baha"} ${n} ${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Has kiçi: garaşylýan ${e.origin} ${n} ${e.minimum.toString()} ${r.unit}` : `Has kiçi: garaşylýan ${e.origin} ${n} ${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Nädogry setir: "${t.prefix}" bilen başlamaly` : t.format === "ends_with" ? `Nädogry setir: "${t.suffix}" bilen gutarmaly` : t.format === "includes" ? `Nädogry setir: "${t.includes}" saklamaly` : t.format === "regex" ? `Nädogry setir: ${t.pattern} nusga laýyk bolmaly` : `Nädogry ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Nädogry san: ${e.divisor} bilen galyndysyz bölünmeli`;
			case "unrecognized_keys": return `Tanalmaýan açar${e.keys.length > 1 ? "lar" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `${e.origin} içinde nädogry açar`;
			case "invalid_union": return "Nädogry baha";
			case "invalid_element": return `${e.origin} içinde nädogry baha`;
			default: return "Nädogry baha";
		}
	};
};
function Gc() {
	return { localeError: Wc() };
}
// node_modules/zod/v4/locales/tr.js
var Kc = () => {
	let e = {
		string: {
			unit: "karakter",
			verb: "olmalı"
		},
		file: {
			unit: "bayt",
			verb: "olmalı"
		},
		array: {
			unit: "öğe",
			verb: "olmalı"
		},
		set: {
			unit: "öğe",
			verb: "olmalı"
		},
		map: {
			unit: "öğe",
			verb: "olmalı"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "girdi",
		email: "e-posta adresi",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO tarih ve saat",
		date: "ISO tarih",
		time: "ISO saat",
		duration: "ISO süre",
		ipv4: "IPv4 adresi",
		ipv6: "IPv6 adresi",
		mac: "MAC adresi",
		cidrv4: "IPv4 aralığı",
		cidrv6: "IPv6 aralığı",
		base64: "base64 ile şifrelenmiş metin",
		base64url: "base64url ile şifrelenmiş metin",
		json_string: "JSON dizesi",
		e164: "E.164 sayısı",
		credit_card: "kredi kartı numarası",
		currency_code: "para birimi kodu",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "Şablon dizesi"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Geçersiz değer: beklenen instanceof ${e.expected}, alınan ${i}` : `Geçersiz değer: beklenen ${t}, alınan ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Geçersiz değer: beklenen ${S(e.values[0])}` : `Geçersiz seçenek: aşağıdakilerden biri olmalı: ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Çok büyük: beklenen ${e.origin ?? "değer"} ${n}${e.maximum.toString()} ${r.unit ?? "öğe"}` : `Çok büyük: beklenen ${e.origin ?? "değer"} ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Çok küçük: beklenen ${e.origin} ${n}${e.minimum.toString()} ${r.unit}` : `Çok küçük: beklenen ${e.origin} ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Geçersiz metin: "${t.prefix}" ile başlamalı` : t.format === "ends_with" ? `Geçersiz metin: "${t.suffix}" ile bitmeli` : t.format === "includes" ? `Geçersiz metin: "${t.includes}" içermeli` : t.format === "regex" ? `Geçersiz metin: ${t.pattern} desenine uymalı` : `Geçersiz ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Geçersiz sayı: ${e.divisor} ile tam bölünebilmeli`;
			case "unrecognized_keys": return `Tanınmayan anahtar${e.keys.length > 1 ? "lar" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `${e.origin} içinde geçersiz anahtar`;
			case "invalid_union": return "Geçersiz değer";
			case "invalid_element": return `${e.origin} içinde geçersiz değer`;
			default: return "Geçersiz değer";
		}
	};
};
function qc() {
	return { localeError: Kc() };
}
// node_modules/zod/v4/locales/uk.js
var Jc = () => {
	let e = {
		string: {
			unit: "символів",
			verb: "матиме"
		},
		file: {
			unit: "байтів",
			verb: "матиме"
		},
		array: {
			unit: "елементів",
			verb: "матиме"
		},
		set: {
			unit: "елементів",
			verb: "матиме"
		},
		map: {
			unit: "елементів",
			verb: "матиме"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "вхідні дані",
		email: "адреса електронної пошти",
		url: "URL",
		emoji: "емодзі",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "дата та час ISO",
		date: "дата ISO",
		time: "час ISO",
		duration: "тривалість ISO",
		ipv4: "адреса IPv4",
		ipv6: "адреса IPv6",
		mac: "адреса MAC",
		cidrv4: "діапазон IPv4",
		cidrv6: "діапазон IPv6",
		base64: "рядок у кодуванні base64",
		base64url: "рядок у кодуванні base64url",
		json_string: "рядок JSON",
		e164: "номер E.164",
		credit_card: "номер кредитної картки",
		currency_code: "код валюти",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "вхідні дані"
	}, r = {
		nan: "NaN",
		number: "число",
		array: "масив"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Неправильні вхідні дані: очікується instanceof ${e.expected}, отримано ${i}` : `Неправильні вхідні дані: очікується ${t}, отримано ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Неправильні вхідні дані: очікується ${S(e.values[0])}` : `Неправильна опція: очікується одне з ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Занадто велике: очікується, що ${e.origin ?? "значення"} ${r.verb} ${n}${e.maximum.toString()} ${r.unit ?? "елементів"}` : `Занадто велике: очікується, що ${e.origin ?? "значення"} буде ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Занадто мале: очікується, що ${e.origin} ${r.verb} ${n}${e.minimum.toString()} ${r.unit}` : `Занадто мале: очікується, що ${e.origin} буде ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Неправильний рядок: повинен починатися з "${t.prefix}"` : t.format === "ends_with" ? `Неправильний рядок: повинен закінчуватися на "${t.suffix}"` : t.format === "includes" ? `Неправильний рядок: повинен містити "${t.includes}"` : t.format === "regex" ? `Неправильний рядок: повинен відповідати шаблону ${t.pattern}` : `Неправильний ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Неправильне число: повинно бути кратним ${e.divisor}`;
			case "unrecognized_keys": return `Нерозпізнаний ключ${e.keys.length > 1 ? "і" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Неправильний ключ у ${e.origin}`;
			case "invalid_union": return "Неправильні вхідні дані";
			case "invalid_element": return `Неправильне значення у ${e.origin}`;
			default: return "Неправильні вхідні дані";
		}
	};
};
function Yc() {
	return { localeError: Jc() };
}
// node_modules/zod/v4/locales/ua.js
function Xc() {
	return Yc();
}
// node_modules/zod/v4/locales/ur.js
var Zc = () => {
	let e = {
		string: {
			unit: "حروف",
			verb: "ہونا"
		},
		file: {
			unit: "بائٹس",
			verb: "ہونا"
		},
		array: {
			unit: "آئٹمز",
			verb: "ہونا"
		},
		set: {
			unit: "آئٹمز",
			verb: "ہونا"
		},
		map: {
			unit: "آئٹمز",
			verb: "ہونا"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "ان پٹ",
		email: "ای میل ایڈریس",
		url: "یو آر ایل",
		emoji: "ایموجی",
		uuid: "یو یو آئی ڈی",
		uuidv4: "یو یو آئی ڈی وی 4",
		uuidv6: "یو یو آئی ڈی وی 6",
		nanoid: "نینو آئی ڈی",
		guid: "جی یو آئی ڈی",
		cuid: "سی یو آئی ڈی",
		cuid2: "سی یو آئی ڈی 2",
		ulid: "یو ایل آئی ڈی",
		xid: "ایکس آئی ڈی",
		ksuid: "کے ایس یو آئی ڈی",
		datetime: "آئی ایس او ڈیٹ ٹائم",
		date: "آئی ایس او تاریخ",
		time: "آئی ایس او وقت",
		duration: "آئی ایس او مدت",
		ipv4: "آئی پی وی 4 ایڈریس",
		ipv6: "آئی پی وی 6 ایڈریس",
		mac: "ایم اے سی ایڈریس",
		cidrv4: "آئی پی وی 4 رینج",
		cidrv6: "آئی پی وی 6 رینج",
		base64: "بیس 64 ان کوڈڈ سٹرنگ",
		base64url: "بیس 64 یو آر ایل ان کوڈڈ سٹرنگ",
		json_string: "جے ایس او این سٹرنگ",
		e164: "ای 164 نمبر",
		credit_card: "کریڈٹ کارڈ نمبر",
		currency_code: "کرنسی کوڈ",
		iban: "IBAN",
		jwt: "جے ڈبلیو ٹی",
		template_literal: "ان پٹ"
	}, r = {
		nan: "NaN",
		number: "نمبر",
		array: "آرے",
		null: "نل"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `غلط ان پٹ: instanceof ${e.expected} متوقع تھا، ${i} موصول ہوا` : `غلط ان پٹ: ${t} متوقع تھا، ${i} موصول ہوا`;
			}
			case "invalid_value": return e.values.length === 1 ? `غلط ان پٹ: ${S(e.values[0])} متوقع تھا` : `غلط آپشن: ${u(e.values, "|")} میں سے ایک متوقع تھا`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `بہت بڑا: ${e.origin ?? "ویلیو"} کے ${n}${e.maximum.toString()} ${r.unit ?? "عناصر"} ہونے متوقع تھے` : `بہت بڑا: ${e.origin ?? "ویلیو"} کا ${n}${e.maximum.toString()} ہونا متوقع تھا`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `بہت چھوٹا: ${e.origin} کے ${n}${e.minimum.toString()} ${r.unit} ہونے متوقع تھے` : `بہت چھوٹا: ${e.origin} کا ${n}${e.minimum.toString()} ہونا متوقع تھا`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `غلط سٹرنگ: "${t.prefix}" سے شروع ہونا چاہیے` : t.format === "ends_with" ? `غلط سٹرنگ: "${t.suffix}" پر ختم ہونا چاہیے` : t.format === "includes" ? `غلط سٹرنگ: "${t.includes}" شامل ہونا چاہیے` : t.format === "regex" ? `غلط سٹرنگ: پیٹرن ${t.pattern} سے میچ ہونا چاہیے` : `غلط ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `غلط نمبر: ${e.divisor} کا مضاعف ہونا چاہیے`;
			case "unrecognized_keys": return `غیر تسلیم شدہ کی${e.keys.length > 1 ? "ز" : ""}: ${u(e.keys, "، ")}`;
			case "invalid_key": return `${e.origin} میں غلط کی`;
			case "invalid_union": return "غلط ان پٹ";
			case "invalid_element": return `${e.origin} میں غلط ویلیو`;
			default: return "غلط ان پٹ";
		}
	};
};
function Qc() {
	return { localeError: Zc() };
}
// node_modules/zod/v4/locales/uz.js
var $c = () => {
	let e = {
		string: {
			unit: "belgi",
			verb: "bo‘lishi kerak"
		},
		file: {
			unit: "bayt",
			verb: "bo‘lishi kerak"
		},
		array: {
			unit: "element",
			verb: "bo‘lishi kerak"
		},
		set: {
			unit: "element",
			verb: "bo‘lishi kerak"
		},
		map: {
			unit: "yozuv",
			verb: "bo‘lishi kerak"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "kirish",
		email: "elektron pochta manzili",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO sana va vaqti",
		date: "ISO sana",
		time: "ISO vaqt",
		duration: "ISO davomiylik",
		ipv4: "IPv4 manzil",
		ipv6: "IPv6 manzil",
		mac: "MAC manzil",
		cidrv4: "IPv4 diapazon",
		cidrv6: "IPv6 diapazon",
		base64: "base64 kodlangan satr",
		base64url: "base64url kodlangan satr",
		json_string: "JSON satr",
		e164: "E.164 raqam",
		credit_card: "kredit karta raqami",
		currency_code: "valyuta kodi",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "kirish"
	}, r = {
		nan: "NaN",
		number: "raqam",
		array: "massiv"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Noto‘g‘ri kirish: kutilgan instanceof ${e.expected}, qabul qilingan ${i}` : `Noto‘g‘ri kirish: kutilgan ${t}, qabul qilingan ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Noto‘g‘ri kirish: kutilgan ${S(e.values[0])}` : `Noto‘g‘ri variant: quyidagilardan biri kutilgan ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Juda katta: kutilgan ${e.origin ?? "qiymat"} ${n}${e.maximum.toString()} ${r.unit} ${r.verb}` : `Juda katta: kutilgan ${e.origin ?? "qiymat"} ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Juda kichik: kutilgan ${e.origin} ${n}${e.minimum.toString()} ${r.unit} ${r.verb}` : `Juda kichik: kutilgan ${e.origin} ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Noto‘g‘ri satr: "${t.prefix}" bilan boshlanishi kerak` : t.format === "ends_with" ? `Noto‘g‘ri satr: "${t.suffix}" bilan tugashi kerak` : t.format === "includes" ? `Noto‘g‘ri satr: "${t.includes}" ni o‘z ichiga olishi kerak` : t.format === "regex" ? `Noto‘g‘ri satr: ${t.pattern} shabloniga mos kelishi kerak` : `Noto‘g‘ri ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Noto‘g‘ri raqam: ${e.divisor} ning karralisi bo‘lishi kerak`;
			case "unrecognized_keys": return `Noma’lum kalit${e.keys.length > 1 ? "lar" : ""}: ${u(e.keys, ", ")}`;
			case "invalid_key": return `${e.origin} dagi kalit noto‘g‘ri`;
			case "invalid_union": return "Noto‘g‘ri kirish";
			case "invalid_element": return `${e.origin} da noto‘g‘ri qiymat`;
			default: return "Noto‘g‘ri kirish";
		}
	};
};
function el() {
	return { localeError: $c() };
}
// node_modules/zod/v4/locales/vi.js
var tl = () => {
	let e = {
		string: {
			unit: "ký tự",
			verb: "có"
		},
		file: {
			unit: "byte",
			verb: "có"
		},
		array: {
			unit: "phần tử",
			verb: "có"
		},
		set: {
			unit: "phần tử",
			verb: "có"
		},
		map: {
			unit: "phần tử",
			verb: "có"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "đầu vào",
		email: "địa chỉ email",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ngày giờ ISO",
		date: "ngày ISO",
		time: "giờ ISO",
		duration: "khoảng thời gian ISO",
		ipv4: "địa chỉ IPv4",
		ipv6: "địa chỉ IPv6",
		mac: "địa chỉ MAC",
		cidrv4: "dải IPv4",
		cidrv6: "dải IPv6",
		base64: "chuỗi mã hóa base64",
		base64url: "chuỗi mã hóa base64url",
		json_string: "chuỗi JSON",
		e164: "số E.164",
		credit_card: "số thẻ tín dụng",
		currency_code: "mã tiền tệ",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "đầu vào"
	}, r = {
		nan: "NaN",
		number: "số",
		array: "mảng"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Đầu vào không hợp lệ: mong đợi instanceof ${e.expected}, nhận được ${i}` : `Đầu vào không hợp lệ: mong đợi ${t}, nhận được ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Đầu vào không hợp lệ: mong đợi ${S(e.values[0])}` : `Tùy chọn không hợp lệ: mong đợi một trong các giá trị ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Quá lớn: mong đợi ${e.origin ?? "giá trị"} ${r.verb} ${n}${e.maximum.toString()} ${r.unit ?? "phần tử"}` : `Quá lớn: mong đợi ${e.origin ?? "giá trị"} ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Quá nhỏ: mong đợi ${e.origin} ${r.verb} ${n}${e.minimum.toString()} ${r.unit}` : `Quá nhỏ: mong đợi ${e.origin} ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Chuỗi không hợp lệ: phải bắt đầu bằng "${t.prefix}"` : t.format === "ends_with" ? `Chuỗi không hợp lệ: phải kết thúc bằng "${t.suffix}"` : t.format === "includes" ? `Chuỗi không hợp lệ: phải bao gồm "${t.includes}"` : t.format === "regex" ? `Chuỗi không hợp lệ: phải khớp với mẫu ${t.pattern}` : `${n[t.format] ?? e.format} không hợp lệ`;
			}
			case "not_multiple_of": return `Số không hợp lệ: phải là bội số của ${e.divisor}`;
			case "unrecognized_keys": return `Khóa không được nhận dạng: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Khóa không hợp lệ trong ${e.origin}`;
			case "invalid_union": return "Đầu vào không hợp lệ";
			case "invalid_element": return `Giá trị không hợp lệ trong ${e.origin}`;
			default: return "Đầu vào không hợp lệ";
		}
	};
};
function nl() {
	return { localeError: tl() };
}
// node_modules/zod/v4/locales/zh-CN.js
var rl = () => {
	let e = {
		string: {
			unit: "字符",
			verb: "包含"
		},
		file: {
			unit: "字节",
			verb: "包含"
		},
		array: {
			unit: "项",
			verb: "包含"
		},
		set: {
			unit: "项",
			verb: "包含"
		},
		map: {
			unit: "项",
			verb: "包含"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "输入",
		email: "电子邮件",
		url: "URL",
		emoji: "表情符号",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO日期时间",
		date: "ISO日期",
		time: "ISO时间",
		duration: "ISO时长",
		ipv4: "IPv4地址",
		ipv6: "IPv6地址",
		mac: "MAC地址",
		cidrv4: "IPv4网段",
		cidrv6: "IPv6网段",
		base64: "base64编码字符串",
		base64url: "base64url编码字符串",
		json_string: "JSON字符串",
		e164: "E.164号码",
		credit_card: "信用卡号",
		currency_code: "货币代码",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "输入"
	}, r = {
		nan: "NaN",
		number: "数字",
		array: "数组",
		null: "空值(null)"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `无效输入：期望 instanceof ${e.expected}，实际接收 ${i}` : `无效输入：期望 ${t}，实际接收 ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `无效输入：期望 ${S(e.values[0])}` : `无效选项：期望以下之一 ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `数值过大：期望 ${e.origin ?? "值"} ${n}${e.maximum.toString()} ${r.unit ?? "个元素"}` : `数值过大：期望 ${e.origin ?? "值"} ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `数值过小：期望 ${e.origin} ${n}${e.minimum.toString()} ${r.unit}` : `数值过小：期望 ${e.origin} ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `无效字符串：必须以 "${t.prefix}" 开头` : t.format === "ends_with" ? `无效字符串：必须以 "${t.suffix}" 结尾` : t.format === "includes" ? `无效字符串：必须包含 "${t.includes}"` : t.format === "regex" ? `无效字符串：必须满足正则表达式 ${t.pattern}` : `无效${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `无效数字：必须是 ${e.divisor} 的倍数`;
			case "unrecognized_keys": return `出现未知的键(key): ${u(e.keys, ", ")}`;
			case "invalid_key": return `${e.origin} 中的键(key)无效`;
			case "invalid_union": return "无效输入";
			case "invalid_element": return `${e.origin} 中包含无效值(value)`;
			default: return "无效输入";
		}
	};
};
function il() {
	return { localeError: rl() };
}
// node_modules/zod/v4/locales/zh-TW.js
var al = () => {
	let e = {
		string: {
			unit: "字元",
			verb: "擁有"
		},
		file: {
			unit: "位元組",
			verb: "擁有"
		},
		array: {
			unit: "項目",
			verb: "擁有"
		},
		set: {
			unit: "項目",
			verb: "擁有"
		},
		map: {
			unit: "項目",
			verb: "擁有"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "輸入",
		email: "郵件地址",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "ISO 日期時間",
		date: "ISO 日期",
		time: "ISO 時間",
		duration: "ISO 期間",
		ipv4: "IPv4 位址",
		ipv6: "IPv6 位址",
		mac: "MAC 位址",
		cidrv4: "IPv4 範圍",
		cidrv6: "IPv6 範圍",
		base64: "base64 編碼字串",
		base64url: "base64url 編碼字串",
		json_string: "JSON 字串",
		e164: "E.164 數值",
		credit_card: "信用卡號",
		currency_code: "貨幣代碼",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "輸入"
	}, r = { nan: "NaN" };
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `無效的輸入值：預期為 instanceof ${e.expected}，但收到 ${i}` : `無效的輸入值：預期為 ${t}，但收到 ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `無效的輸入值：預期為 ${S(e.values[0])}` : `無效的選項：預期為以下其中之一 ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `數值過大：預期 ${e.origin ?? "值"} 應為 ${n}${e.maximum.toString()} ${r.unit ?? "個元素"}` : `數值過大：預期 ${e.origin ?? "值"} 應為 ${n}${e.maximum.toString()}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `數值過小：預期 ${e.origin} 應為 ${n}${e.minimum.toString()} ${r.unit}` : `數值過小：預期 ${e.origin} 應為 ${n}${e.minimum.toString()}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `無效的字串：必須以 "${t.prefix}" 開頭` : t.format === "ends_with" ? `無效的字串：必須以 "${t.suffix}" 結尾` : t.format === "includes" ? `無效的字串：必須包含 "${t.includes}"` : t.format === "regex" ? `無效的字串：必須符合格式 ${t.pattern}` : `無效的 ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `無效的數字：必須為 ${e.divisor} 的倍數`;
			case "unrecognized_keys": return `無法識別的鍵值${e.keys.length > 1 ? "們" : ""}：${u(e.keys, "、")}`;
			case "invalid_key": return `${e.origin} 中有無效的鍵值`;
			case "invalid_union": return "無效的輸入值";
			case "invalid_element": return `${e.origin} 中有無效的值`;
			default: return "無效的輸入值";
		}
	};
};
function ol() {
	return { localeError: al() };
}
// node_modules/zod/v4/locales/yo.js
var sl = () => {
	let e = {
		string: {
			unit: "àmi",
			verb: "ní"
		},
		file: {
			unit: "bytes",
			verb: "ní"
		},
		array: {
			unit: "nkan",
			verb: "ní"
		},
		set: {
			unit: "nkan",
			verb: "ní"
		},
		map: {
			unit: "nkan",
			verb: "ní"
		}
	};
	function t(t) {
		return e[t] ?? null;
	}
	let n = {
		regex: "ẹ̀rọ ìbáwọlé",
		email: "àdírẹ́sì ìmẹ́lì",
		url: "URL",
		emoji: "emoji",
		uuid: "UUID",
		uuidv4: "UUIDv4",
		uuidv6: "UUIDv6",
		nanoid: "nanoid",
		guid: "GUID",
		cuid: "cuid",
		cuid2: "cuid2",
		ulid: "ULID",
		xid: "XID",
		ksuid: "KSUID",
		datetime: "àkókò ISO",
		date: "ọjọ́ ISO",
		time: "àkókò ISO",
		duration: "àkókò tó pé ISO",
		ipv4: "àdírẹ́sì IPv4",
		ipv6: "àdírẹ́sì IPv6",
		mac: "àdírẹ́sì MAC",
		cidrv4: "àgbègbè IPv4",
		cidrv6: "àgbègbè IPv6",
		base64: "ọ̀rọ̀ tí a kọ́ ní base64",
		base64url: "ọ̀rọ̀ base64url",
		json_string: "ọ̀rọ̀ JSON",
		e164: "nọ́mbà E.164",
		credit_card: "nọmba kaadi gbese",
		currency_code: "koodu owó",
		iban: "IBAN",
		jwt: "JWT",
		template_literal: "ẹ̀rọ ìbáwọlé"
	}, r = {
		nan: "NaN",
		number: "nọ́mbà",
		array: "akopọ"
	};
	return (e) => {
		switch (e.code) {
			case "invalid_type": {
				let t = r[e.expected] ?? e.expected, n = E(e.input), i = r[n] ?? n;
				return /^[A-Z]/.test(e.expected) ? `Ìbáwọlé aṣìṣe: a ní láti fi instanceof ${e.expected}, àmọ̀ a rí ${i}` : `Ìbáwọlé aṣìṣe: a ní láti fi ${t}, àmọ̀ a rí ${i}`;
			}
			case "invalid_value": return e.values.length === 1 ? `Ìbáwọlé aṣìṣe: a ní láti fi ${S(e.values[0])}` : `Àṣàyàn aṣìṣe: yan ọ̀kan lára ${u(e.values, "|")}`;
			case "too_big": {
				let n = e.inclusive ? "<=" : "<", r = t(e.origin);
				return r ? `Tó pọ̀ jù: a ní láti jẹ́ pé ${e.origin ?? "iye"} ${r.verb} ${n}${e.maximum} ${r.unit}` : `Tó pọ̀ jù: a ní láti jẹ́ ${n}${e.maximum}`;
			}
			case "too_small": {
				let n = e.inclusive ? ">=" : ">", r = t(e.origin);
				return r ? `Kéré ju: a ní láti jẹ́ pé ${e.origin} ${r.verb} ${n}${e.minimum} ${r.unit}` : `Kéré ju: a ní láti jẹ́ ${n}${e.minimum}`;
			}
			case "invalid_format": {
				let t = e;
				return t.format === "starts_with" ? `Ọ̀rọ̀ aṣìṣe: gbọ́dọ̀ bẹ̀rẹ̀ pẹ̀lú "${t.prefix}"` : t.format === "ends_with" ? `Ọ̀rọ̀ aṣìṣe: gbọ́dọ̀ parí pẹ̀lú "${t.suffix}"` : t.format === "includes" ? `Ọ̀rọ̀ aṣìṣe: gbọ́dọ̀ ní "${t.includes}"` : t.format === "regex" ? `Ọ̀rọ̀ aṣìṣe: gbọ́dọ̀ bá àpẹẹrẹ mu ${t.pattern}` : `Aṣìṣe: ${n[t.format] ?? e.format}`;
			}
			case "not_multiple_of": return `Nọ́mbà aṣìṣe: gbọ́dọ̀ jẹ́ èyà pípín ti ${e.divisor}`;
			case "unrecognized_keys": return `Bọtìnì àìmọ̀: ${u(e.keys, ", ")}`;
			case "invalid_key": return `Bọtìnì aṣìṣe nínú ${e.origin}`;
			case "invalid_union": return "Ìbáwọlé aṣìṣe";
			case "invalid_element": return `Iye aṣìṣe nínú ${e.origin}`;
			default: return "Ìbáwọlé aṣìṣe";
		}
	};
};
function cl() {
	return { localeError: sl() };
}
// node_modules/zod/v4/locales/index.js
var ll = /* @__PURE__ */ t({
	ar: () => Ho,
	az: () => Wo,
	be: () => qo,
	bg: () => Yo,
	bn: () => Zo,
	ca: () => $o,
	ckb: () => ts,
	cs: () => rs,
	da: () => as,
	de: () => ss,
	el: () => ls,
	en: () => ds,
	eo: () => ps,
	es: () => hs,
	fa: () => _s,
	fi: () => ys,
	fr: () => xs,
	frCA: () => Cs,
	gu: () => Ts,
	he: () => Ds,
	hi: () => ks,
	hr: () => js,
	hu: () => Ns,
	hy: () => Ls,
	id: () => zs,
	is: () => Vs,
	it: () => Us,
	ja: () => Gs,
	ka: () => qs,
	kh: () => Xs,
	km: () => Ys,
	kn: () => Qs,
	ko: () => ec,
	lt: () => ic,
	mk: () => oc,
	ms: () => cc,
	ne: () => uc,
	nl: () => fc,
	nn: () => mc,
	no: () => gc,
	ota: () => vc,
	pl: () => Sc,
	ps: () => bc,
	pt: () => wc,
	ptBR: () => Ec,
	ro: () => Oc,
	ru: () => jc,
	sk: () => Nc,
	sl: () => Fc,
	sv: () => Lc,
	ta: () => zc,
	tg: () => Vc,
	th: () => Uc,
	tk: () => Gc,
	tr: () => qc,
	ua: () => Xc,
	uk: () => Yc,
	ur: () => Qc,
	uz: () => el,
	vi: () => nl,
	yo: () => cl,
	zhCN: () => il,
	zhTW: () => ol
}), ul, dl = /*@__PURE__*/ Symbol("ZodOutput"), fl = /*@__PURE__*/ Symbol("ZodInput"), pl = class {
	constructor() {
		this._map = /* @__PURE__ */ new WeakMap(), this._idmap = /* @__PURE__ */ new Map();
	}
	add(e, ...t) {
		let n = t[0];
		return this._map.set(e, n), n && typeof n == "object" && "id" in n && this._idmap.set(n.id, e), this;
	}
	clear() {
		return this._map = /* @__PURE__ */ new WeakMap(), this._idmap = /* @__PURE__ */ new Map(), this;
	}
	remove(e) {
		let t = this._map.get(e);
		return t && typeof t == "object" && "id" in t && this._idmap.delete(t.id), this._map.delete(e), this;
	}
	get(e) {
		let t = e._zod.parent;
		if (t) {
			let n = { ...this.get(t) ?? {} };
			delete n.id;
			let r = {
				...n,
				...this._map.get(e)
			};
			return Object.keys(r).length ? r : void 0;
		}
		return this._map.get(e);
	}
	has(e) {
		return this._map.has(e);
	}
};
function ml() {
	return new pl();
}
(ul = globalThis).__zod_globalRegistry ?? (ul.__zod_globalRegistry = ml());
var F = globalThis.__zod_globalRegistry, I = Symbol.for("zod.compile.invalid"), hl = Symbol.for("zod.compile.fallback"), gl = class extends Error {
	constructor(e = "z.compile does not support async refinements, transforms, or checks") {
		super(e), this.name = "ZodCompileAsyncError";
	}
}, L = class extends Error {
	constructor(e, t = !0) {
		super(`z.compile does not support ${e}; this schema must use the runtime parser`), this.name = "ZodCompileUnsupportedError", this.islandable = t;
	}
};
function _l(e, t) {
	try {
		return xl(e, { assertOnly: !0 });
	} catch {
		return t;
	}
}
function vl(e, t) {
	try {
		let t = xl(e), n = yl(e, t);
		return n._zod.bag.validator = _l(e, t), n;
	} catch (n) {
		if (t?.strict) throw n;
		return e;
	}
}
function yl(e, t) {
	if (Po(e)) throw new L("a schema whose subtree contains a reference cycle");
	let n = b(e), r = e._zod.run, i = r.__originalRun ?? r, a = (e, n) => {
		if (n?.async || n?.direction === "backward" || n?.skipChecks || n?.[hl] || n && Bo(n, e.value)) return i(e, n);
		let r = t(e.value);
		return r === I ? (n && (n[hl] = !0), i(e, n)) : (e.value = r, e);
	};
	return a.__originalRun = i, n._zod.bag.fallbackRun = i, n._zod.bag.validator = t, n._zod.run = a, r.__originalRun || bl(n, e, t), n;
}
function bl(e, t, n) {
	let r = e, i = t;
	if (typeof i.safeParse == "function") {
		let e = i.safeParse;
		r.safeParse = (t, r) => {
			let i = n(t);
			return i === I ? e(t, r) : {
				success: !0,
				data: i
			};
		};
	}
	if (typeof i.parse == "function") {
		let e = i.parse;
		r.parse = (t, r) => {
			let i = n(t);
			return i === I ? e(t, r) : i;
		};
	}
}
function xl(e, t) {
	let n = !0;
	try {
		n = Po(e);
	} catch {}
	if (n) throw new L("a schema whose subtree contains a reference cycle");
	let r = {
		constants: /* @__PURE__ */ new Map(),
		constantCounter: 0,
		varCounter: 0,
		definite: !0
	}, i = new ei(["input"]), a = V(i, r, e, "input", !t?.assertOnly);
	i.write(a === null ? "return true;" : `return ${a};`);
	let o = ["INVALID", ...r.constants.keys()], s = [I, ...r.constants.values()], c = i.content.join("\n"), l = t?.debug ? o.length > 0 ? `// Constants: ${o.join(", ")}\n${c}` : c : "", u = Function, d = `return (input) => {\n${c}\n}`, f;
	try {
		f = new u(...o, d)(...s);
	} catch (e) {
		throw new L(`this schema (generated code failed to evaluate: ${e.message})`);
	}
	return t?.debug && (f.code = l), f.definite = r.definite, f;
}
function R(e, t) {
	for (let [n, r] of e.constants) if (r === t) return n;
	let n = `c${e.constantCounter++}`;
	return e.constants.set(n, t), n;
}
function Sl(e, t) {
	return e.definite = !1, R(e, t);
}
function z(e) {
	return `v${e.varCounter++}`;
}
function Cl(e, t) {
	let n = e._zod.run({
		value: t,
		issues: []
	}, {});
	if (n && typeof n.then == "function") return I;
	let r = n;
	return r.issues.length === 0 ? r.value : I;
}
function B(e, t, n, r, i = !0) {
	let a = e.content.length, o = t.constants.size, s = t.constantCounter, c = t.varCounter;
	try {
		return V(e, t, n, r, i);
	} catch (i) {
		if (!(i instanceof L) || !i.islandable) throw i;
		if (e.content.length = a, t.constants.size > o) {
			let e = Array.from(t.constants.keys()).slice(o);
			for (let n of e) t.constants.delete(n);
		}
		return t.constantCounter = s, t.varCounter = c, wl(e, t, n, r);
	}
}
function wl(e, t, n, r) {
	t.definite = !1;
	let i = R(t, n), a = R(t, Cl), o = z(t);
	return e.write(`const ${o} = ${a}(${i}, ${r});`), e.write(`if (${o} === INVALID) return INVALID;`), o;
}
var Tl = /* @__PURE__ */ new Set([
	"max_size",
	"min_size",
	"size_equals",
	"max_length",
	"min_length",
	"length_equals"
]);
function El(e, t, n, r) {
	let i = n._zod.def.checks;
	if (!i || i.length === 0) return r;
	let a = r;
	for (let n of i) {
		let r = n._zod.def;
		if (r.when && !Tl.has(r.check)) throw new L("check with a custom \"when\" condition");
		switch (r.check) {
			case "greater_than":
				Al(e, t, r, a);
				break;
			case "less_than":
				jl(e, t, r, a);
				break;
			case "multiple_of":
				Ml(e, t, r, a);
				break;
			case "number_format":
				Nl(e, r, a);
				break;
			case "min_length": {
				let n = Ol(r.minimum, "min_length"), i = Dl(e, t, a, `${a}.length >= ${n} && ${a}.length < ${r.minimum * 2}`);
				e.write(`if (${i} < ${n}) return INVALID;`);
				break;
			}
			case "max_length": {
				let n = Ol(r.maximum, "max_length"), i = Dl(e, t, a, `${a}.length > ${n}`);
				e.write(`if (${i} > ${n}) return INVALID;`);
				break;
			}
			case "length_equals": {
				let n = Ol(r.length, "length_equals"), i = Dl(e, t, a, `${a}.length >= ${n} && ${a}.length <= ${r.length * 2}`);
				e.write(`if (${i} !== ${n}) return INVALID;`);
				break;
			}
			case "min_size":
				e.write(`if (${a}.size < ${Ol(r.minimum, "min_size")}) return INVALID;`);
				break;
			case "max_size":
				e.write(`if (${a}.size > ${Ol(r.maximum, "max_size")}) return INVALID;`);
				break;
			case "size_equals":
				e.write(`if (${a}.size !== ${Ol(r.size, "size_equals")}) return INVALID;`);
				break;
			case "string_format":
				a = Ul(e, t, r, a);
				break;
			case "custom":
				a = Vl(e, t, n, a);
				break;
			case "bigint_format":
				Pl(e, r, a);
				break;
			case "mime_type":
				Fl(e, t, r, a);
				break;
			case "property":
				Ll(e, t, r, a);
				break;
			case "properties":
				Il(e, t, r, a);
				break;
			case "overwrite": {
				let r = z(t);
				Rl(e, t, n, a, r), a = r;
				break;
			}
			default: throw new L(`check type ${r.check}`);
		}
	}
	return a;
}
function Dl(e, t, n, r) {
	let i = R(t, Ue), a = z(t);
	return e.write(`const ${a} = typeof ${n} === "string" && ${r} ? ${i}(${n}) : ${n}.length;`), a;
}
function Ol(e, t) {
	if (typeof e != "number" || !Number.isFinite(e)) throw new L(`${t} bound of type ${typeof e}`);
	return `${e}`;
}
function kl(e, t) {
	if (typeof t == "bigint") return `${t}n`;
	if (typeof t == "number") {
		if (Number.isNaN(t)) throw new L("comparison check with NaN bound");
		return `${t}`;
	}
	if (t instanceof Date) {
		if (Number.isNaN(t.getTime())) throw new L("comparison check with Invalid Date bound");
		return R(e, t);
	}
	throw new L(`comparison check bound of type ${typeof t}`);
}
function Al(e, t, n, r) {
	let i = n.inclusive ? "<" : "<=";
	e.write(`if (${r} ${i} ${kl(t, n.value)}) return INVALID;`);
}
function jl(e, t, n, r) {
	let i = n.inclusive ? ">" : ">=";
	e.write(`if (${r} ${i} ${kl(t, n.value)}) return INVALID;`);
}
function Ml(e, t, n, r) {
	if (typeof n.value == "bigint") {
		if (n.value === BigInt(0)) throw new L("multiple_of check with a zero divisor");
		e.write(`if (${r} % ${n.value}n !== 0n) return INVALID;`);
	} else {
		let i = R(t, g);
		e.write(`if (${i}(${r}, ${Ol(n.value, "multiple_of")}) !== 0) return INVALID;`);
	}
}
function Nl(e, t, n) {
	let r = t.format;
	switch (r) {
		case "safeint":
			e.write(`if (!Number.isSafeInteger(${n})) return INVALID;`);
			break;
		case "int32":
			e.write(`if (!Number.isInteger(${n}) || ${n} < -2147483648 || ${n} > 2147483647) return INVALID;`);
			break;
		case "uint32":
			e.write(`if (!Number.isInteger(${n}) || ${n} < 0 || ${n} > 4294967295) return INVALID;`);
			break;
		case "float32":
			e.write(`if (!Number.isFinite(${n}) || ${n} < -3.4028234663852886e38 || ${n} > 3.4028234663852886e38) return INVALID;`);
			break;
		case "float64":
			e.write(`if (!Number.isFinite(${n})) return INVALID;`);
			break;
		default: throw new L(`number format ${r}`);
	}
}
function Pl(e, t, n) {
	let r = t.format;
	if (r) switch (r) {
		case "int64":
			e.write(`if (${n} < -9223372036854775808n || ${n} > 9223372036854775807n) return INVALID;`);
			break;
		case "uint64":
			e.write(`if (${n} < 0n || ${n} > 18446744073709551615n) return INVALID;`);
			break;
		default: throw new L(`bigint format ${r}`);
	}
}
function Fl(e, t, n, r) {
	let i = n.mime;
	if (i && i.length > 0) {
		let n = R(t, new Set(i));
		e.write(`if (!${n}.has(${r}.type)) return INVALID;`);
	}
}
function Il(e, t, n, r) {
	if (n.when) throw new L("check with a custom \"when\" condition");
	e.write(`if (${r} == null) return INVALID;`);
	let i = n.shape;
	for (let n of Reflect.ownKeys(i)) {
		let a = typeof n == "symbol" ? R(t, n) : y(n), o = z(t);
		e.write(`const ${o} = ${r}[${a}];`), B(e, t, i[n], o, !1);
	}
}
function Ll(e, t, n, r) {
	let i = `${r}[${JSON.stringify(n.property)}]`;
	V(e, t, n.schema, i);
}
function Rl(e, t, n, r, i) {
	let a = n._zod.def.tx;
	if (!a) throw new L("overwrite check without a transform function");
	if (ju(a)) throw new gl("z.compile: async overwrite transforms are not supported");
	let o = R(t, a);
	e.write(`const ${i} = ${o}(${r});`);
}
function zl() {
	throw new vt();
}
function Bl(e) {
	this.issues.push(e);
}
function Vl(e, t, n, r) {
	let i = n._zod.def;
	if (i.fn) {
		if (ju(i.fn)) throw new gl("z.compile: async .refine() predicates are not supported");
		let n = Sl(t, i.fn), a = R(t, zl), o = z(t);
		return e.write(`const ${o} = ${n}(${r});`), e.write(`if (${o} instanceof Promise) ${a}();`), e.write(`if (!${o}) return INVALID;`), r;
	}
	if (n._zod.check) {
		if (ju(n._zod.check)) throw new gl("z.compile: async .superRefine() / check functions are not supported");
		let i = n._zod.check, a = Sl(t, (e) => {
			let t = {
				value: e,
				issues: [],
				addIssue: Bl
			};
			return i(t) instanceof Promise && zl(), t.issues.length === 0 ? t.value : I;
		}), o = z(t);
		return e.write(`const ${o} = ${a}(${r});`), e.write(`if (${o} === INVALID) return INVALID;`), o;
	}
	throw new L("custom check without a predicate or check function");
}
var Hl = /* @__PURE__ */ new Set([
	"cidrv4",
	"cuid",
	"cuid2",
	"date",
	"datetime",
	"duration",
	"e164",
	"email",
	"emoji",
	"ends_with",
	"guid",
	"includes",
	"ipv4",
	"ksuid",
	"lowercase",
	"mac",
	"nanoid",
	"regex",
	"starts_with",
	"time",
	"ulid",
	"uppercase",
	"uuid",
	"xid"
]);
function Ul(e, t, n, r, i = !0) {
	let a = n.format;
	if (a === "base64") {
		let n = R(t, Ii);
		return e.write(`if (!${n}(${r})) return INVALID;`), r;
	}
	if (a === "base64url") {
		let n = R(t, Bi);
		return e.write(`if (!${n}(${r})) return INVALID;`), r;
	}
	if (a === "jwt") {
		let i = R(t, Xi), a = R(t, n.alg ?? null);
		return e.write(`if (!${i}(${r}, ${a})) return INVALID;`), r;
	}
	if (a === "ipv6") {
		let n = R(t, Ai);
		return e.write(`if (!${n}(${r})) return INVALID;`), r;
	}
	if (a === "cidrv6") {
		let n = R(t, Pi);
		return e.write(`if (!${n}(${r})) return INVALID;`), r;
	}
	if (a === "credit_card") {
		let n = R(t, Gi);
		return e.write(`if (!${n}(${r})) return INVALID;`), r;
	}
	if (a === "iban") {
		let n = R(t, Ji);
		return e.write(`if (!${n}(${r})) return INVALID;`), r;
	}
	let o = n;
	if (a === "url" || a === "httpurl" || o.normalize || o.hostname !== void 0 || o.protocol !== void 0) {
		let a = R(t, ui), s = R(t, n), c = z(t), l = z(t);
		if (e.write(`const ${c} = ${r}.trim();`), e.write(`const ${l} = ${a}(${c}, ${s});`), e.write(`if (typeof ${l} === "number") return INVALID;`), o.hostname !== void 0) {
			let n = R(t, mi);
			e.write(`if (!${n}(${l}, ${s}.hostname)) return INVALID;`);
		}
		if (o.protocol !== void 0) {
			let n = R(t, hi);
			e.write(`if (!${n}(${l}, ${s}.protocol)) return INVALID;`);
		}
		if (!i) return null;
		let u = z(t), d = o.normalize ? `${l}.href` : `${R(t, pi)}(${c})`;
		return e.write(`const ${u} = ${d};`), u;
	}
	let s = n.fn;
	if (s) {
		if (ju(s)) throw new L(`async string format ${a}`);
		let n = R(t, s);
		return e.write(`if (!${n}(${r})) return INVALID;`), r;
	}
	if (Hl.has(a) && n.pattern) {
		let i = R(t, n.pattern);
		return e.write(`${i}.lastIndex = 0;`), e.write(`if (!${i}.test(${r})) return INVALID;`), r;
	}
	let c = n.format;
	switch (c) {
		case "regex": throw new L("regex format without a pattern");
		case "lowercase":
			e.write(`if (${r} !== ${r}.toLowerCase()) return INVALID;`);
			break;
		case "uppercase":
			e.write(`if (${r} !== ${r}.toUpperCase()) return INVALID;`);
			break;
		case "includes":
			e.write(`if (!${r}.includes(${y(n.includes)})) return INVALID;`);
			break;
		case "starts_with": {
			let t = n.prefix;
			e.write(`if (${r}.slice(0, ${t.length}) !== ${y(t)}) return INVALID;`);
			break;
		}
		case "ends_with": {
			let t = n.suffix;
			e.write(`if (${r}.slice(-${t.length}) !== ${y(t)}) return INVALID;`);
			break;
		}
		default: throw new L(`string format ${c}`);
	}
	return r;
}
function V(e, t, n, r, i = !0) {
	let a = n._zod.def, o = a.type;
	if (a.coerce) throw new L(`coercion (z.coerce.${o}())`);
	let s = i || !!a.checks?.length, c;
	switch (o) {
		case "string":
			c = Wl(e, t, n, r, s);
			break;
		case "number":
			c = Gl(e, n, r);
			break;
		case "boolean":
			c = Kl(e, r);
			break;
		case "bigint":
			c = ql(e, n, r);
			break;
		case "symbol":
			c = Jl(e, r);
			break;
		case "undefined":
			c = Yl(e, r);
			break;
		case "null":
			c = Xl(e, r);
			break;
		case "any":
		case "unknown":
			c = r;
			break;
		case "never":
			e.write("return INVALID;"), c = r;
			break;
		case "void":
			c = Zl(e, r);
			break;
		case "nan":
			c = Ql(e, r);
			break;
		case "date":
			c = $l(e, r);
			break;
		case "object":
			c = eu(e, t, n, r, s);
			break;
		case "optional":
			c = tu(e, t, n, r, s);
			break;
		case "nullable":
			c = cu(e, t, n, r, s);
			break;
		case "array":
			c = uu(e, t, n, r, s);
			break;
		case "literal":
			c = du(e, t, n, r);
			break;
		case "enum":
			c = fu(e, t, n, r);
			break;
		case "readonly": {
			let i = pu(e, t, n, r), a = z(t);
			e.write(`const ${a} = Object.freeze(${i});`), c = a;
			break;
		}
		case "success":
			pu(e, t, n, r), c = "true";
			break;
		case "default":
		case "prefault":
			c = mu(e, t, n, r);
			break;
		case "nonoptional":
			c = hu(e, t, n, r);
			break;
		case "tuple":
			c = gu(e, t, n, r);
			break;
		case "union":
			c = vu(e, t, n, r);
			break;
		case "intersection":
			c = xu(e, t, n, r);
			break;
		case "record":
			c = Su(e, t, n, r);
			break;
		case "map":
			c = Tu(e, t, n, r);
			break;
		case "set":
			c = Eu(e, t, n, r);
			break;
		case "file":
			c = Du(e, r);
			break;
		case "template_literal":
			c = Ou(e, t, n, r);
			break;
		case "lazy":
			c = ku(e, t, n, r);
			break;
		case "pipe":
			c = Au(e, t, n, r);
			break;
		case "custom":
			c = Mu(e, t, n, r);
			break;
		case "transform":
			c = Fu(e, t, n, r);
			break;
		case "catch":
			c = Pu(e, t, n, r);
			break;
		default: throw new L(`schema type ${o}`);
	}
	return c === null ? null : El(e, t, n, c);
}
function Wl(e, t, n, r, i = !0) {
	e.write(`if (typeof ${r} !== "string") return INVALID;`);
	let a = n._zod.def;
	return a.format === void 0 ? r : Ul(e, t, a, r, i);
}
function Gl(e, t, n) {
	e.write(`if (typeof ${n} !== "number" || !Number.isFinite(${n})) return INVALID;`);
	let r = t._zod.def;
	return r.check === "number_format" && r.format && Nl(e, { format: r.format }, n), n;
}
function Kl(e, t) {
	return e.write(`if (typeof ${t} !== "boolean") return INVALID;`), t;
}
function ql(e, t, n) {
	e.write(`if (typeof ${n} !== "bigint") return INVALID;`);
	let r = t._zod.def;
	if (r.format) switch (r.format) {
		case "int64":
			e.write(`if (${n} < -9223372036854775808n || ${n} > 9223372036854775807n) return INVALID;`);
			break;
		case "uint64": e.write(`if (${n} < 0n || ${n} > 18446744073709551615n) return INVALID;`);
	}
	return n;
}
function Jl(e, t) {
	return e.write(`if (typeof ${t} !== "symbol") return INVALID;`), t;
}
function Yl(e, t) {
	return e.write(`if (${t} !== undefined) return INVALID;`), t;
}
function Xl(e, t) {
	return e.write(`if (${t} !== null) return INVALID;`), t;
}
function Zl(e, t) {
	return e.write(`if (${t} !== undefined) return INVALID;`), t;
}
function Ql(e, t) {
	return e.write(`if (typeof ${t} !== "number" || !Number.isNaN(${t})) return INVALID;`), t;
}
function $l(e, t) {
	return e.write(`if (!(${t} instanceof Date) || Number.isNaN(${t}.getTime())) return INVALID;`), t;
}
function eu(e, t, n, r, i = !0) {
	let a = n._zod.def;
	e.write(`if (typeof ${r} !== "object" || ${r} === null || Array.isArray(${r})) return INVALID;`);
	let o = a.shape, s = Object.keys(o), c = Object.getOwnPropertySymbols(o), l = c.length ? [...s, ...c] : s, u = (e) => typeof e == "symbol" ? R(t, e) : y(e), d = (e) => typeof e == "symbol" ? `[${u(e)}]` : y(e), f = o;
	if (s.includes("__proto__")) throw new L("object shape key \"__proto__\"");
	let p = /* @__PURE__ */ new Map();
	for (let n of l) {
		let a = f[n], o = u(n), s = z(t);
		if (e.write(`const ${s} = ${r}[${o}];`), a._zod.optin !== void 0) {
			let i = z(t);
			e.write(`let ${i} = (() => {`), e.indented((e) => {
				let n = B(e, t, a, s);
				e.write(`return ${n};`);
			}), e.write("})();"), a._zod.optout === "optional" ? (e.write(`if (${i} === INVALID) {`), e.indented((e) => {
				e.write(`if (${o} in ${r}) return INVALID;`), e.write(`${i} = undefined;`);
			}), e.write("}")) : e.write(`if (${i} === INVALID) return INVALID;`), p.set(n, i);
		} else {
			ru(a) && e.write(`if (!(${o} in ${r})) return INVALID;`);
			let c = B(e, t, a, s, i);
			c !== null && p.set(n, c);
		}
	}
	let m = a.catchall, h = "none";
	if (m) {
		let t = m._zod.def.type;
		if (t === "never") {
			let t = s.map((e) => `k !== ${y(e)}`).join(" && ") || "true";
			e.write(`for (const k in ${r}) {`), e.indented((e) => {
				e.write(`if (${t}) return INVALID;`);
			}), e.write("}");
		} else h = (t === "unknown" || t === "any") && !m._zod.def.checks?.length ? "passthrough" : "schema";
	}
	let g = z(t), ee = l.some((e) => ou(f[e]) || au(f[e]));
	if (!i) {
		if (h === "schema") {
			let n = s.length > 0 ? R(t, new Set(s)) : null;
			e.write(`for (const k in ${r}) {`), e.indented((e) => {
				e.write("if (k === \"__proto__\") continue;"), n && e.write(`if (${n}.has(k)) continue;`);
				let i = z(t);
				e.write(`const ${i} = ${r}[k];`), B(e, t, m, i, !1);
			}), e.write("}");
		}
		return null;
	}
	if (ee) {
		e.write(`const ${g} = {};`);
		for (let t of l) {
			let n = u(t), i = p.get(t);
			au(f[t]) ? e.write(`if (${n} in ${r}) ${g}[${n}] = ${i};`) : ou(f[t]) ? e.write(`if (${i} !== undefined || ${n} in ${r}) ${g}[${n}] = ${i};`) : e.write(`${g}[${n}] = ${i};`);
		}
	} else {
		let t = l.map((e) => `${d(e)}: ${p.get(e)}`).join(", ");
		e.write(`const ${g} = { ${t} };`);
	}
	if (h !== "none") {
		let n = s.length > 0 ? R(t, new Set(s)) : null;
		e.write(`for (const k in ${r}) {`), e.indented((e) => {
			if (e.write("if (k === \"__proto__\") continue;"), n && e.write(`if (${n}.has(k)) continue;`), h === "passthrough") e.write(`${g}[k] = ${r}[k];`);
			else {
				let n = z(t);
				e.write(`const ${n} = ${r}[k];`);
				let i = B(e, t, m, n);
				e.write(`${g}[k] = ${i};`);
			}
		}), e.write("}");
	}
	return g;
}
function tu(e, t, n, r, i = !0) {
	let a = n._zod.def;
	if (nu(n)) return V(e, t, a.innerType, r, i);
	if (a.innerType._zod.optin === "defaulted") {
		let n = z(t), i = z(t);
		return e.write(`let ${n};`), e.write(`if (${r} === undefined) {`), e.indented((e) => {
			e.write(`const ${i} = (() => {`), e.indented((e) => {
				let n = V(e, t, a.innerType, r);
				e.write(`return ${n};`);
			}), e.write("})();"), e.write(`if (${i} !== INVALID) ${n} = ${i};`);
		}), e.write("} else {"), e.indented((e) => {
			let i = V(e, t, a.innerType, r);
			e.write(`${n} = ${i};`);
		}), e.write("}"), n;
	}
	let o = i ? z(t) : null;
	return o && e.write(`let ${o};`), e.write(`if (${r} !== undefined) {`), e.indented((e) => {
		let n = V(e, t, a.innerType, r, i);
		o && n !== null && e.write(`${o} = ${n};`);
	}), e.write("}"), o;
}
function nu(e) {
	return e._zod.traits?.has("$ZodExactOptional") === !0;
}
function ru(e) {
	return e._zod.optin === void 0 && iu(e);
}
function iu(e) {
	if (e._zod.def.coerce) return !0;
	let t = e._zod.def;
	switch (t.type) {
		case "any":
		case "unknown":
		case "undefined":
		case "void":
		case "default":
		case "prefault":
		case "transform":
		case "custom":
		case "lazy": return !0;
		case "string":
		case "number":
		case "boolean":
		case "bigint":
		case "symbol":
		case "null":
		case "never":
		case "nan":
		case "date":
		case "object":
		case "array":
		case "tuple":
		case "record":
		case "map":
		case "set":
		case "file":
		case "template_literal": return !1;
		case "nonoptional": return t.innerType ? iu(t.innerType) : !1;
		case "literal": return !!t.values?.includes(void 0);
		case "enum": return !!e._zod.values?.has(void 0);
		case "optional":
		case "nullable":
		case "readonly":
		case "success": return !t.innerType || iu(t.innerType);
		case "catch": return !0;
		case "union": return !t.options || t.options.some(iu);
		case "intersection": return !t.left || !t.right || iu(t.left) && iu(t.right);
		case "pipe": return !t.in || iu(t.in);
		default: return !0;
	}
}
function au(e) {
	return e._zod.optin === "optional" && e._zod.optout === "optional";
}
function ou(e) {
	return (e._zod.optin !== "defaulted" || e._zod.optout === "optional") && su(e);
}
function su(e) {
	let t = e._zod.def;
	switch (t.type) {
		case "string":
		case "number":
		case "boolean":
		case "bigint":
		case "symbol":
		case "null":
		case "nan":
		case "date":
		case "object":
		case "array":
		case "tuple":
		case "record":
		case "map":
		case "set":
		case "file":
		case "template_literal":
		case "never":
		case "success": return !1;
		case "literal": return !!t.values?.includes(void 0);
		case "enum": return !!e._zod.values?.has(void 0);
		case "optional": return !0;
		case "nullable":
		case "readonly":
		case "nonoptional": return !t.innerType || su(t.innerType);
		case "union": return !t.options || t.options.some(su);
		case "intersection": return !t.left || !t.right || su(t.left) || su(t.right);
		case "pipe": return !t.out || su(t.out);
		default: return !0;
	}
}
function cu(e, t, n, r, i = !0) {
	let a = n._zod.def, o = i ? z(t) : null;
	return o && e.write(`let ${o} = null;`), e.write(`if (${r} !== null) {`), e.indented((e) => {
		let n = V(e, t, a.innerType, r, i);
		o && n !== null && e.write(`${o} = ${n};`);
	}), e.write("}"), o;
}
function lu(e, t, n) {
	for (let [r, i] of ze(t._zod.def.checks)) e.write(`if (${n} > ${Ol(r, i._zod.def.check)}) return INVALID;`);
}
function uu(e, t, n, r, i = !0) {
	let a = n._zod.def;
	e.write(`if (!Array.isArray(${r})) return INVALID;`), lu(e, n, `${r}.length`);
	let o = i ? z(t) : null, s = z(t), c = z(t);
	return o && e.write(`const ${o} = new Array(${r}.length);`), e.write(`for (let ${s} = 0; ${s} < ${r}.length; ${s}++) {`), e.indented((e) => {
		e.write(`const ${c} = ${r}[${s}];`);
		let n = B(e, t, a.element, c, i);
		o && n !== null && e.write(`${o}[${s}] = ${n};`);
	}), e.write("}"), o;
}
function du(e, t, n, r) {
	let i = n._zod.def.values;
	if (i.length !== 1) {
		let n = R(t, new Set(i));
		return e.write(`if (!${n}.has(${r})) return INVALID;`), r;
	}
	let a = i[0];
	if (typeof a == "number" && Number.isNaN(a)) {
		let n = R(t, new Set(i));
		return e.write(`if (!${n}.has(${r})) return INVALID;`), r;
	}
	if (typeof a == "string") e.write(`if (${r} !== ${y(a)}) return INVALID;`);
	else if (typeof a == "number" || typeof a == "boolean") e.write(`if (${r} !== ${a}) return INVALID;`);
	else if (a === null) e.write(`if (${r} !== null) return INVALID;`);
	else if (a === void 0) e.write(`if (${r} !== undefined) return INVALID;`);
	else if (typeof a == "bigint") e.write(`if (${r} !== ${a}n) return INVALID;`);
	else throw new L(`literal type ${typeof a}`);
	return r;
}
function fu(e, t, n, r) {
	let i = n._zod.values;
	if (!i) throw new L("enum schema without enumerated values");
	let a = R(t, i);
	return e.write(`if (!${a}.has(${r})) return INVALID;`), r;
}
function pu(e, t, n, r) {
	let i = n._zod.def;
	return V(e, t, i.innerType, r);
}
function mu(e, t, n, r) {
	let i = n._zod.def, a = Object.getOwnPropertyDescriptor(n._zod.def, "defaultValue") ? () => n._zod.def.defaultValue : void 0;
	if (n._zod.def.type === "prefault") {
		if (!a) return V(e, t, i.innerType, r);
		let n = R(t, a), o = z(t);
		return e.write(`let ${o} = ${r};`), e.write(`if (${r} === undefined) ${o} = ${n}();`), V(e, t, i.innerType, o);
	}
	let o = z(t);
	if (a) {
		let n = R(t, a), s = R(t, ve);
		e.write(`let ${o};`), e.write(`if (${r} === undefined) {`), e.indented((e) => {
			e.write(`${o} = ${s}(${n}());`);
		}), e.write("} else {"), e.indented((e) => {
			let a = V(e, t, i.innerType, r);
			e.write(`${o} = ${a} === undefined ? ${s}(${n}()) : ${a};`);
		}), e.write("}");
	} else e.write(`let ${o};`), e.write(`if (${r} !== undefined) {`), e.indented((e) => {
		let n = V(e, t, i.innerType, r);
		e.write(`${o} = ${n};`);
	}), e.write("}");
	return o;
}
function hu(e, t, n, r) {
	let i = n._zod.def, a = V(e, t, i.innerType, r), o = z(t);
	return e.write(`const ${o} = ${a};`), e.write(`if (${o} === undefined) return INVALID;`), o;
}
function gu(e, t, n, r) {
	let i = n._zod.def, a = i.items, o = i.rest;
	e.write(`if (!Array.isArray(${r})) return INVALID;`);
	let s = _u(a, "optin"), c = _u(a, "optout");
	o ? e.write(`if (${r}.length < ${s}) return INVALID;`) : e.write(`if (${r}.length < ${s} || ${r}.length > ${a.length}) return INVALID;`);
	let l = z(t);
	e.write(`const ${l} = [];`);
	for (let n = 0; n < a.length; n++) {
		let i = a[n];
		if (n >= c) e.write(`if (${l}.length === ${n}) {`), e.indented((e) => {
			e.write(`if (${n} < ${r}.length) {`), e.indented((e) => {
				let a = z(t);
				e.write(`const ${a} = ${r}[${n}];`);
				let o = B(e, t, i, a);
				e.write(`${l}[${n}] = ${o};`);
			}), e.write("} else {"), e.indented((e) => {
				if (au(i)) {
					e.write(`${l}.length = ${n};`);
					return;
				}
				let r = z(t), a = z(t);
				e.write(`const ${r} = undefined;`), e.write(`const ${a} = (() => {`), e.indented((e) => {
					let n = B(e, t, i, r);
					e.write(`return ${n};`);
				}), e.write("})();"), e.write(`if (${a} === INVALID || ${a} === undefined) ${l}.length = ${n};`), e.write(`else ${l}[${n}] = ${a};`);
			}), e.write("}");
		}), e.write("}");
		else {
			let a = z(t);
			e.write(`const ${a} = ${r}[${n}];`);
			let o = B(e, t, i, a);
			e.write(`${l}[${n}] = ${o};`);
		}
	}
	if (o) {
		let n = z(t), i = z(t);
		e.write(`for (let ${n} = ${a.length}; ${n} < ${r}.length; ${n}++) {`), e.indented((e) => {
			e.write(`const ${i} = ${r}[${n}];`);
			let a = B(e, t, o, i);
			e.write(`${l}[${n}] = ${a};`);
		}), e.write("}");
	}
	return l;
}
function _u(e, t) {
	for (let n = e.length - 1; n >= 0; n--) if (!(t === "optin" ? e[n]._zod.optin !== void 0 : e[n]._zod.optout === "optional")) return n + 1;
	return 0;
}
function vu(e, t, n, r) {
	let i = n._zod.def, a = i.options;
	if (i.discriminator) return yu(e, t, i, r);
	if (i.inclusive === !1) throw new L("exclusive unions (z.xor)");
	if (a.length === 0) return e.write("return INVALID;"), r;
	if (a.length === 1) return V(e, t, a[0], r);
	if (a.every((e) => e._zod.def.type === "literal" && !e._zod.def.checks?.length)) {
		let n = R(t, new Set(a.flatMap((e) => e._zod.def.values)));
		return e.write(`if (!${n}.has(${r})) return INVALID;`), r;
	}
	let o = z(t);
	e.write(`let ${o};`);
	for (let n = 0; n < a.length; n++) {
		let i = a[n];
		n === 0 ? e.write(`${o} = (() => {`) : e.write(`if (${o} === INVALID) ${o} = (() => {`), e.indented((e) => {
			let n = V(e, t, i, r);
			e.write(`return ${n};`);
		}), e.write("})();");
	}
	return e.write(`if (${o} === INVALID) return INVALID;`), o;
}
function yu(e, t, n, r) {
	if (n.unionFallback) throw new L("discriminated union with unionFallback");
	if (n.options.length === 0) return e.write("return INVALID;"), r;
	let i = z(t), a = z(t);
	e.write(`const ${i} = ${r}?.[${y(n.discriminator)}];`), e.write(`let ${a};`);
	let o = !0, s = /* @__PURE__ */ new Set();
	for (let c of n.options) {
		let l = c._zod.propValues?.[n.discriminator];
		if (!l || l.size === 0) throw new L("discriminated union option without static discriminator values");
		for (let e of l) {
			if (s.has(e)) throw new L(`duplicate discriminator value ${String(e)}`);
			s.add(e);
		}
		let u = Array.from(l, (e) => bu(t, i, e)), d = o ? "if" : "else if";
		e.write(`${d} (${u.join(" || ")}) {`), e.indented((e) => {
			let n = V(e, t, c, r);
			e.write(`${a} = ${n};`);
		}), e.write("}"), o = !1;
	}
	return e.write("else { return INVALID; }"), a;
}
function bu(e, t, n) {
	if (typeof n == "string") return `${t} === ${y(n)}`;
	if (typeof n == "number") return Number.isNaN(n) ? `Number.isNaN(${t})` : `${t} === ${n}`;
	if (typeof n == "boolean") return `${t} === ${n}`;
	if (n === null) return `${t} === null`;
	if (n === void 0) return `${t} === undefined`;
	if (typeof n == "bigint") return `${t} === ${n}n`;
	if (typeof n == "symbol") return `${t} === ${R(e, n)}`;
	throw new L(`literal discriminator value ${String(n)}`);
}
function xu(e, t, n, r) {
	let i = n._zod.def;
	t.definite = !1;
	let a = B(e, t, i.left, r), o = B(e, t, i.right, r), s = R(t, Ma), c = z(t);
	return e.write(`const ${c} = ${s}(${a}, ${o});`), e.write(`if (!${c}.valid) return INVALID;`), `${c}.data`;
}
function Su(e, t, n, r) {
	let i = n._zod.def, a = R(t, _e);
	e.write(`if (!${a}(${r})) return INVALID;`);
	let o = z(t), s = z(t), c = z(t);
	e.write(`const ${o} = {};`);
	let l = i, u = l.partial ? void 0 : i.keyType._zod.values;
	if (u) {
		let n = [];
		for (let a of u) {
			if (typeof a != "string" && typeof a != "number" && typeof a != "symbol") throw new L(`record key value ${String(a)}`);
			let s = typeof a == "number" ? a.toString() : a;
			if (s === "__proto__") throw new L("record key \"__proto__\"");
			n.push(s);
			let c = R(t, a), l = V(e, t, i.keyType, c), u = z(t);
			e.write(`const ${u} = ${r}[${wu(t, s)}];`);
			let d = B(e, t, i.valueType, u);
			e.write(`${o}[${l}] = ${d};`);
		}
		let a = R(t, new Set(n));
		return e.write(`for (const ${s} in ${r}) {`), e.indented((e) => {
			e.write(`if (${a}.has(${s})) continue;`), l.mode === "loose" ? e.write(`if (${s} !== "__proto__") ${o}[${s}] = ${r}[${s}];`) : e.write("return INVALID;");
		}), e.write("}"), o;
	}
	let d = i.keyType._zod.def;
	if (d.type !== "string" || d.format !== void 0 || d.coerce || (d.checks?.length ?? 0) !== 0) {
		let n = i.mode === "loose", a = xl(i.keyType);
		a.definite === !1 && (t.definite = !1);
		let c = R(t, a), l = R(t, ir), u = z(t);
		return Cu(e, t, r, s, (e) => {
			e.write(`let ${u} = ${c}(${s});`), e.write(`if (${u} === INVALID && typeof ${s} === "string" && ${l}.test(${s})) ${u} = ${c}(Number(${s}));`), n ? e.write(`if (${u} === INVALID) { ${o}[${s}] = ${r}[${s}]; continue; }`) : e.write(`if (${u} === INVALID) return INVALID;`), e.write(`if (${u} === "__proto__") continue;`);
			let a = z(t);
			e.write(`const ${a} = ${r}[${s}];`);
			let d = B(e, t, i.valueType, a);
			e.write(`${o}[${u}] = ${d};`);
		}), o;
	}
	return Cu(e, t, r, s, (e) => {
		e.write(`const ${c} = ${r}[${s}];`);
		let n = B(e, t, i.valueType, c);
		e.write(`${o}[${s}] = ${n};`);
	}, "return INVALID;"), o;
}
function Cu(e, t, n, r, i, a) {
	let o = R(t, Object.prototype.propertyIsEnumerable), s = z(t), c = z(t), l = z(t);
	e.write(`const ${s} = Object.getOwnPropertySymbols(${n});`), e.write(`const ${c} = Object.getOwnPropertyNames(${n});`), e.write(`for (let ${l} = 0; ${l} < ${c}.length; ${l}++) {`), e.indented((e) => {
		e.write(`const ${r} = ${c}[${l}];`), e.write(`if (${r} === "__proto__" || !${o}.call(${n}, ${r})) continue;`), i(e);
	}), e.write("}"), e.write(`for (let ${l} = 0; ${l} < ${s}.length; ${l}++) {`), e.indented((e) => {
		e.write(`const ${r} = ${s}[${l}];`), e.write(`if (!${o}.call(${n}, ${r})) continue;`), a ? e.write(a) : i(e);
	}), e.write("}");
}
function wu(e, t) {
	return typeof t == "string" ? y(t) : R(e, t);
}
function Tu(e, t, n, r) {
	let i = n._zod.def;
	e.write(`if (!(${r} instanceof Map)) return INVALID;`), lu(e, n, `${r}.size`);
	let a = z(t), o = z(t), s = z(t);
	return e.write(`const ${a} = new Map();`), e.write(`for (const [${o}, ${s}] of ${r}) {`), e.indented((e) => {
		let n = V(e, t, i.keyType, o), r = V(e, t, i.valueType, s);
		e.write(`${a}.set(${n}, ${r});`);
	}), e.write("}"), a;
}
function Eu(e, t, n, r) {
	let i = n._zod.def;
	e.write(`if (!(${r} instanceof Set)) return INVALID;`), lu(e, n, `${r}.size`);
	let a = z(t), o = z(t);
	return e.write(`const ${a} = new Set();`), e.write(`for (const ${o} of ${r}) {`), e.indented((e) => {
		let n = V(e, t, i.valueType, o);
		e.write(`${a}.add(${n});`);
	}), e.write("}"), a;
}
function Du(e, t) {
	return e.write(`if (!(${t} instanceof File)) return INVALID;`), t;
}
function Ou(e, t, n, r) {
	e.write(`if (typeof ${r} !== "string") return INVALID;`);
	let i = n._zod.pattern;
	if (i) {
		let n = R(t, i);
		e.write(`${n}.lastIndex = 0;`), e.write(`if (!${n}.test(${r})) return INVALID;`);
	}
	return r;
}
function ku(e, t, n, r) {
	let i = n._zod.def, a = Sl(t, i.getter), o = R(t, { parser: null });
	e.write(`if (!${o}.parser) {`), e.indented((e) => {
		e.write(`const inner = ${a}();`), e.write(`${o}.parser = function(input) {`), e.indented((e) => {
			e.write("const result = inner._zod.run({ value: input, issues: [] }, {});"), e.write("return result.issues.length === 0 ? result.value : INVALID;");
		}), e.write("};");
	}), e.write("}");
	let s = z(t);
	return e.write(`const ${s} = ${o}.parser(${r});`), e.write(`if (${s} === INVALID) return INVALID;`), s;
}
function Au(e, t, n, r) {
	let i = n._zod.def, a = V(e, t, i.in, r);
	if (i.transform) {
		if (ju(i.transform)) throw new gl("z.compile: async transforms in pipes are not supported");
		let n = i.transform, r = Sl(t, (e) => {
			let t = {
				value: e,
				issues: [],
				addIssue: Bl
			}, r = n(e, t);
			return r instanceof Promise ? I : t.issues.length === 0 ? r : I;
		}), o = z(t);
		return e.write(`const ${o} = ${r}(${a});`), e.write(`if (${o} === INVALID) return INVALID;`), V(e, t, i.out, o);
	}
	return V(e, t, i.out, a);
}
function ju(e) {
	return typeof e == "function" && (e.constructor.name === "AsyncFunction" || e[Symbol.toStringTag] === "AsyncFunction");
}
function Mu(e, t, n, r) {
	let i = n._zod.def;
	if (i.fn) {
		if (ju(i.fn)) throw new gl("z.compile: async custom predicates are not supported");
		let n = Sl(t, i.fn), a = R(t, zl), o = z(t);
		e.write(`const ${o} = ${n}(${r});`), e.write(`if (${o} instanceof Promise) ${a}();`), e.write(`if (!${o}) return INVALID;`);
	} else throw new L("custom schema without a predicate function");
	return r;
}
function Nu(e, t, n) {
	let r = e._zod.run({
		value: n,
		issues: []
	}, {});
	if (r && typeof r.then == "function") return I;
	let i = r;
	return i.issues.length === 0 ? i.value : t();
}
function Pu(e, t, n, r) {
	let i = n._zod.def;
	if (!i.catchValue["~constantCatch"]) throw new L("catch with a callback (only a constant catch value compiles)", !1);
	let a = z(t);
	e.write(`let ${a} = (() => {`), e.indented((e) => {
		let n = B(e, t, i.innerType, r);
		e.write(`return ${n};`);
	}), e.write("})();");
	let o = R(t, i.innerType), s = Sl(t, i.catchValue), c = R(t, Nu);
	return e.write(`if (${a} === INVALID) {`), e.indented((e) => {
		e.write(`${a} = ${c}(${o}, ${s}, ${r});`), e.write(`if (${a} === INVALID) return INVALID;`);
	}), e.write("}"), a;
}
function Fu(e, t, n, r) {
	let i = n._zod.def;
	if (i.transform) {
		if (ju(i.transform)) throw new gl("z.compile: async transforms are not supported");
		let n = i.transform, a = Sl(t, (e) => {
			let t = {
				value: e,
				issues: [],
				addIssue: Bl
			}, r = n(e, t);
			return r instanceof Promise ? I : t.issues.length === 0 ? r : I;
		}), o = z(t);
		return e.write(`const ${o} = ${a}(${r});`), e.write(`if (${o} === INVALID) return INVALID;`), o;
	}
	return r;
}
// node_modules/zod/v4/core/api.js
function Iu(e) {
	return e.checks &&= [...e.checks], e;
}
// @__NO_SIDE_EFFECTS__
function Lu(e, t) {
	return new e(Iu({
		type: "string",
		...x(t)
	}));
}
// @__NO_SIDE_EFFECTS__
function Ru(e, t) {
	return new e(Iu({
		type: "string",
		coerce: !0,
		...x(t)
	}));
}
// @__NO_SIDE_EFFECTS__
function zu(e, t) {
	return new e({
		type: "string",
		format: "email",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Bu(e, t) {
	return new e({
		type: "string",
		format: "guid",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Vu(e, t) {
	return new e({
		type: "string",
		format: "uuid",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Hu(e, t) {
	return new e({
		type: "string",
		format: "uuid",
		check: "string_format",
		abort: !1,
		version: "v4",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Uu(e, t) {
	return new e({
		type: "string",
		format: "uuid",
		check: "string_format",
		abort: !1,
		version: "v6",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Wu(e, t) {
	return new e({
		type: "string",
		format: "uuid",
		check: "string_format",
		abort: !1,
		version: "v7",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Gu(e, t) {
	return new e({
		type: "string",
		format: "url",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Ku(e, t) {
	return new e({
		type: "string",
		format: "emoji",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function qu(e, t) {
	return new e({
		type: "string",
		format: "nanoid",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Ju(e, t) {
	return new e({
		type: "string",
		format: "cuid",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Yu(e, t) {
	return new e({
		type: "string",
		format: "cuid2",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Xu(e, t) {
	return new e({
		type: "string",
		format: "ulid",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Zu(e, t) {
	return new e({
		type: "string",
		format: "xid",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Qu(e, t) {
	return new e({
		type: "string",
		format: "ksuid",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function $u(e, t) {
	return new e({
		type: "string",
		format: "ipv4",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function ed(e, t) {
	return new e({
		type: "string",
		format: "ipv6",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function td(e, t) {
	return new e({
		type: "string",
		format: "mac",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function nd(e, t) {
	return new e({
		type: "string",
		format: "cidrv4",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function rd(e, t) {
	return new e({
		type: "string",
		format: "cidrv6",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function id(e, t) {
	return new e({
		type: "string",
		format: "base64",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function ad(e, t) {
	return new e({
		type: "string",
		format: "base64url",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function od(e, t) {
	return new e({
		type: "string",
		format: "e164",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function sd(e, t) {
	return new e({
		type: "string",
		format: "credit_card",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function cd(e, t) {
	return new e({
		type: "string",
		format: "iban",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function ld(e, t) {
	return new e({
		type: "string",
		format: "jwt",
		check: "string_format",
		abort: !1,
		...x(t)
	});
}
var ud = {
	Any: null,
	Minute: -1,
	Second: 0,
	Millisecond: 3,
	Microsecond: 6
};
// @__NO_SIDE_EFFECTS__
function dd(e, t) {
	return new e({
		type: "string",
		format: "datetime",
		check: "string_format",
		offset: !1,
		local: !1,
		precision: null,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function fd(e, t) {
	return new e({
		type: "string",
		format: "date",
		check: "string_format",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function pd(e, t) {
	return new e({
		type: "string",
		format: "time",
		check: "string_format",
		precision: null,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function md(e, t) {
	return new e({
		type: "string",
		format: "duration",
		check: "string_format",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function hd(e, t) {
	return new e(Iu({
		type: "number",
		checks: [],
		...x(t)
	}));
}
// @__NO_SIDE_EFFECTS__
function gd(e, t) {
	return new e(Iu({
		type: "number",
		coerce: !0,
		checks: [],
		...x(t)
	}));
}
// @__NO_SIDE_EFFECTS__
function _d(e, t) {
	return new e({
		type: "number",
		check: "number_format",
		abort: !1,
		format: "safeint",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function vd(e, t) {
	return new e({
		type: "number",
		check: "number_format",
		abort: !1,
		format: "float32",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function yd(e, t) {
	return new e({
		type: "number",
		check: "number_format",
		abort: !1,
		format: "float64",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function bd(e, t) {
	return new e({
		type: "number",
		check: "number_format",
		abort: !1,
		format: "int32",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function xd(e, t) {
	return new e({
		type: "number",
		check: "number_format",
		abort: !1,
		format: "uint32",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Sd(e, t) {
	return new e({
		type: "boolean",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Cd(e, t) {
	return new e({
		type: "boolean",
		coerce: !0,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function wd(e, t) {
	return new e({
		type: "bigint",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Td(e, t) {
	return new e({
		type: "bigint",
		coerce: !0,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Ed(e, t) {
	return new e({
		type: "bigint",
		check: "bigint_format",
		abort: !1,
		format: "int64",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Dd(e, t) {
	return new e({
		type: "bigint",
		check: "bigint_format",
		abort: !1,
		format: "uint64",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Od(e, t) {
	return new e({
		type: "symbol",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function kd(e, t) {
	return new e({
		type: "undefined",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Ad(e, t) {
	return new e({
		type: "null",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function jd(e) {
	return new e({ type: "any" });
}
// @__NO_SIDE_EFFECTS__
function Md(e) {
	return new e({ type: "unknown" });
}
// @__NO_SIDE_EFFECTS__
function Nd(e, t) {
	return new e({
		type: "never",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Pd(e, t) {
	return new e({
		type: "void",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Fd(e, t) {
	return new e({
		type: "date",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Id(e, t) {
	return new e({
		type: "date",
		coerce: !0,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Ld(e, t) {
	return new e({
		type: "nan",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Rd(e, t) {
	return new jr({
		check: "less_than",
		...x(t),
		value: e,
		inclusive: !1
	});
}
// @__NO_SIDE_EFFECTS__
function H(e, t) {
	return new jr({
		check: "less_than",
		...x(t),
		value: e,
		inclusive: !0
	});
}
// @__NO_SIDE_EFFECTS__
function zd(e, t) {
	return new Mr({
		check: "greater_than",
		...x(t),
		value: e,
		inclusive: !1
	});
}
// @__NO_SIDE_EFFECTS__
function U(e, t) {
	return new Mr({
		check: "greater_than",
		...x(t),
		value: e,
		inclusive: !0
	});
}
// @__NO_SIDE_EFFECTS__
function Bd(e) {
	return /* @__PURE__ */ zd(0, e);
}
// @__NO_SIDE_EFFECTS__
function Vd(e) {
	return /* @__PURE__ */ Rd(0, e);
}
// @__NO_SIDE_EFFECTS__
function Hd(e) {
	return /* @__PURE__ */ H(0, e);
}
// @__NO_SIDE_EFFECTS__
function Ud(e) {
	return /* @__PURE__ */ U(0, e);
}
// @__NO_SIDE_EFFECTS__
function Wd(e, t) {
	return new Nr({
		check: "multiple_of",
		...x(t),
		value: e
	});
}
// @__NO_SIDE_EFFECTS__
function Gd(e, t) {
	return new Ir({
		check: "max_size",
		...x(t),
		maximum: e
	});
}
// @__NO_SIDE_EFFECTS__
function Kd(e, t) {
	return new Lr({
		check: "min_size",
		...x(t),
		minimum: e
	});
}
// @__NO_SIDE_EFFECTS__
function qd(e, t) {
	return new Rr({
		check: "size_equals",
		...x(t),
		size: e
	});
}
// @__NO_SIDE_EFFECTS__
function Jd(e, t) {
	return new zr({
		check: "max_length",
		...x(t),
		maximum: e
	});
}
// @__NO_SIDE_EFFECTS__
function Yd(e, t) {
	return new Br({
		check: "min_length",
		...x(t),
		minimum: e
	});
}
// @__NO_SIDE_EFFECTS__
function Xd(e, t) {
	return new Vr({
		check: "length_equals",
		...x(t),
		length: e
	});
}
// @__NO_SIDE_EFFECTS__
function Zd(e, t) {
	return new Ur({
		check: "string_format",
		format: "regex",
		...x(t),
		pattern: e
	});
}
// @__NO_SIDE_EFFECTS__
function Qd(e) {
	return new Wr({
		check: "string_format",
		format: "lowercase",
		...x(e)
	});
}
// @__NO_SIDE_EFFECTS__
function $d(e) {
	return new Gr({
		check: "string_format",
		format: "uppercase",
		...x(e)
	});
}
// @__NO_SIDE_EFFECTS__
function ef(e, t) {
	return new Kr({
		check: "string_format",
		format: "includes",
		...x(t),
		includes: e
	});
}
// @__NO_SIDE_EFFECTS__
function tf(e, t) {
	return new qr({
		check: "string_format",
		format: "starts_with",
		...x(t),
		prefix: e
	});
}
// @__NO_SIDE_EFFECTS__
function nf(e, t) {
	return new Jr({
		check: "string_format",
		format: "ends_with",
		...x(t),
		suffix: e
	});
}
// @__NO_SIDE_EFFECTS__
function rf(e, t, n) {
	return new Xr({
		check: "property",
		property: e,
		schema: t,
		...x(n)
	});
}
// @__NO_SIDE_EFFECTS__
function af(e, t) {
	return new Zr({
		check: "properties",
		shape: e,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function of(e, t) {
	return new Qr({
		check: "mime_type",
		mime: e,
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function sf(e) {
	return new $r({
		check: "overwrite",
		tx: e
	});
}
// @__NO_SIDE_EFFECTS__
function cf(e) {
	return /* @__PURE__ */ sf((t) => t.normalize(e));
}
// @__NO_SIDE_EFFECTS__
function lf() {
	return /* @__PURE__ */ sf((e) => e.trim());
}
// @__NO_SIDE_EFFECTS__
function uf() {
	return /* @__PURE__ */ sf((e) => e.toLowerCase());
}
// @__NO_SIDE_EFFECTS__
function df() {
	return /* @__PURE__ */ sf((e) => e.toUpperCase());
}
// @__NO_SIDE_EFFECTS__
function ff() {
	return /* @__PURE__ */ sf((e) => pe(e));
}
// @__NO_SIDE_EFFECTS__
function pf(e, t, n) {
	return new e({
		type: "array",
		element: t,
		...x(n)
	});
}
// @__NO_SIDE_EFFECTS__
function mf(e, t, n) {
	return new e({
		type: "union",
		options: t,
		...x(n)
	});
}
function hf(e, t, n) {
	return new e({
		type: "union",
		options: t,
		inclusive: !1,
		...x(n)
	});
}
// @__NO_SIDE_EFFECTS__
function gf(e, t, n, r) {
	return new e({
		type: "union",
		options: n,
		discriminator: t,
		...x(r)
	});
}
// @__NO_SIDE_EFFECTS__
function _f(e, t, n) {
	return new e({
		type: "intersection",
		left: t,
		right: n
	});
}
// @__NO_SIDE_EFFECTS__
function vf(e, t, n, r) {
	let i = n instanceof N;
	return new e({
		type: "tuple",
		items: t,
		rest: i ? n : null,
		...x(i ? r : n)
	});
}
// @__NO_SIDE_EFFECTS__
function yf(e, t, n, r) {
	return new e({
		type: "record",
		keyType: t,
		valueType: n,
		...x(r)
	});
}
// @__NO_SIDE_EFFECTS__
function bf(e, t, n, r) {
	return new e({
		type: "map",
		keyType: t,
		valueType: n,
		...x(r)
	});
}
// @__NO_SIDE_EFFECTS__
function xf(e, t, n) {
	return new e({
		type: "set",
		valueType: t,
		...x(n)
	});
}
// @__NO_SIDE_EFFECTS__
function Sf(e, t, n) {
	return new e({
		type: "enum",
		entries: Array.isArray(t) ? Object.fromEntries(t.map((e) => [e, e])) : t,
		...x(n)
	});
}
// @__NO_SIDE_EFFECTS__
function Cf(e, t, n) {
	return new e({
		type: "enum",
		entries: t,
		...x(n)
	});
}
// @__NO_SIDE_EFFECTS__
function wf(e, t, n) {
	return new e({
		type: "literal",
		values: Array.isArray(t) ? t : [t],
		...x(n)
	});
}
// @__NO_SIDE_EFFECTS__
function Tf(e, t) {
	return new e({
		type: "file",
		...x(t)
	});
}
// @__NO_SIDE_EFFECTS__
function Ef(e, t) {
	return new e({
		type: "transform",
		transform: t
	});
}
// @__NO_SIDE_EFFECTS__
function Df(e, t) {
	return new e({
		type: "optional",
		innerType: t
	});
}
// @__NO_SIDE_EFFECTS__
function Of(e, t) {
	return new e({
		type: "nullable",
		innerType: t
	});
}
// @__NO_SIDE_EFFECTS__
function kf(e, t, n) {
	return new e({
		type: "default",
		innerType: t,
		get defaultValue() {
			return typeof n == "function" ? n() : ve(n);
		}
	});
}
// @__NO_SIDE_EFFECTS__
function Af(e, t, n) {
	return new e({
		type: "nonoptional",
		innerType: t,
		...x(n)
	});
}
// @__NO_SIDE_EFFECTS__
function jf(e, t) {
	return new e({
		type: "success",
		innerType: t
	});
}
// @__NO_SIDE_EFFECTS__
function Mf(e, t, n) {
	return new e({
		type: "catch",
		innerType: t,
		catchValue: typeof n == "function" ? n : dt(n)
	});
}
// @__NO_SIDE_EFFECTS__
function Nf(e, t, n) {
	return new e({
		type: "pipe",
		in: t,
		out: n
	});
}
// @__NO_SIDE_EFFECTS__
function Pf(e, t) {
	return new e({
		type: "readonly",
		innerType: t
	});
}
// @__NO_SIDE_EFFECTS__
function Ff(e, t, n) {
	return new e({
		type: "template_literal",
		parts: t,
		...x(n)
	});
}
// @__NO_SIDE_EFFECTS__
function If(e, t) {
	return new e({
		type: "lazy",
		getter: t
	});
}
// @__NO_SIDE_EFFECTS__
function Lf(e, t) {
	return new e({
		type: "promise",
		innerType: t
	});
}
// @__NO_SIDE_EFFECTS__
function Rf(e, t, n) {
	let r = x(n);
	return r.abort ??= !0, new e({
		type: "custom",
		check: "custom",
		fn: t,
		...r
	});
}
// @__NO_SIDE_EFFECTS__
function zf(e, t, n) {
	return new e({
		type: "custom",
		check: "custom",
		fn: t,
		...x(n)
	});
}
// @__NO_SIDE_EFFECTS__
function Bf(e, t) {
	let n = /* @__PURE__ */ Vf((t) => (t.addIssue = (e) => {
		if (typeof e == "string") t.issues.push(Ge(e, t.value, n._zod.def));
		else {
			let r = e;
			r.fatal && (r.continue = !1), r.code ??= "custom", "input" in r || (r.input = t.value), r.inst ??= n, r.continue ??= !n._zod.def.abort, t.issues.push(Ge(r));
		}
	}, e(t.value, t)), t);
	return n;
}
// @__NO_SIDE_EFFECTS__
function Vf(e, t) {
	let n = new M({
		check: "custom",
		...x(t)
	});
	return n._zod.check = e, n;
}
// @__NO_SIDE_EFFECTS__
function Hf(e) {
	let t = new M({ check: "describe" });
	return t._zod.onattach = [(t) => {
		let n = F.get(t) ?? {};
		F.add(t, {
			...n,
			description: e
		});
	}], t._zod.check = () => {}, t;
}
// @__NO_SIDE_EFFECTS__
function Uf(e) {
	let t = new M({ check: "meta" });
	return t._zod.onattach = [(t) => {
		let n = F.get(t) ?? {};
		F.add(t, {
			...n,
			...e
		});
	}], t._zod.check = () => {}, t;
}
// @__NO_SIDE_EFFECTS__
function Wf(e, t) {
	let n = x(t), r = n.truthy ?? [
		"true",
		"1",
		"yes",
		"on",
		"y",
		"enabled"
	], i = n.falsy ?? [
		"false",
		"0",
		"no",
		"off",
		"n",
		"disabled"
	];
	n.case !== "sensitive" && (r = r.map((e) => typeof e == "string" ? e.toLowerCase() : e), i = i.map((e) => typeof e == "string" ? e.toLowerCase() : e));
	let a = new Set(r), o = new Set(i), s = e.Codec ?? co, c = e.Boolean ?? ta, l = new s({
		type: "pipe",
		in: new (e.String ?? ai)({
			type: "string",
			error: n.error
		}),
		out: new c({
			type: "boolean",
			error: n.error
		}),
		transform: ((e, t) => {
			let r = e;
			return n.case !== "sensitive" && (r = r.toLowerCase()), a.has(r) ? !0 : !o.has(r) && (t.issues.push({
				code: "invalid_value",
				expected: "stringbool",
				values: [...a, ...o],
				input: t.value,
				inst: l,
				continue: !1
			}), {});
		}),
		reverseTransform: ((e, t) => e === !0 ? r[0] || "true" : i[0] || "false"),
		error: n.error
	});
	return l._zod.bag.truthy = r, l._zod.bag.falsy = i, l._zod.bag.case = n.case ?? "insensitive", l;
}
// @__NO_SIDE_EFFECTS__
function Gf(e, t, n, r = {}) {
	let i = x(r), a = {
		check: "string_format",
		type: "string",
		format: t,
		fn: typeof n == "function" ? n : (e) => n.test(e),
		...i
	};
	return n instanceof RegExp && (a.pattern = n), new e(a);
}
// node_modules/zod/v4/core/to-json-schema.js
function Kf(e, ...t) {
	for (let n of t) for (let t of Reflect.ownKeys(n)) Object.prototype.propertyIsEnumerable.call(n, t) && _(e, t, n[t]);
	return e;
}
function qf(e) {
	let t = e?.target ?? "draft-2020-12";
	return t === "draft-4" && (t = "draft-04"), t === "draft-7" && (t = "draft-07"), {
		processors: e.processors ?? {},
		metadataRegistry: e?.metadata ?? F,
		target: t,
		unrepresentable: e?.unrepresentable ?? "throw",
		override: e?.override ?? (() => {}),
		io: e?.io ?? "output",
		counter: 0,
		seen: /* @__PURE__ */ new Map(),
		sharedDefsExtractedFor: void 0,
		sharedEmitDoneFor: void 0,
		cycles: e?.cycles ?? "ref",
		reused: e?.reused ?? "inline",
		intersections: [],
		deferred: [],
		external: e?.external ?? void 0
	};
}
function W(e, t, n, r, i) {
	let a = typeof t.unrepresentable == "function" ? t.unrepresentable({
		zodSchema: e,
		path: r.path,
		message: i
	}) : t.unrepresentable;
	if (a === "any") return !1;
	if (a === void 0 || a === "throw") throw Error(i);
	return Object.assign(n, a), !0;
}
function G(e, t, n = {
	path: [],
	schemaPath: []
}) {
	var r;
	let i = e._zod.def, a = t.seen.get(e);
	if (a) return a.count++, n.schemaPath.includes(e) && (a.cycle = n.path), a.schema;
	let o = {
		schema: {},
		count: 1,
		cycle: void 0,
		path: n.path
	};
	t.seen.set(e, o), t.sharedDefsExtractedFor = void 0, t.sharedEmitDoneFor = void 0;
	let s = e._zod.toJSONSchema?.();
	if (s) o.schema = s;
	else {
		let r = {
			...n,
			schemaPath: [...n.schemaPath, e],
			path: n.path
		};
		if (e._zod.processJSONSchema) e._zod.processJSONSchema(t, o.schema, r);
		else {
			let n = o.schema, a = t.processors[i.type];
			if (!a) throw Error(`[toJSONSchema]: Non-representable type encountered: ${i.type}`);
			a(e, t, n, r);
		}
		let a = e._zod.parent;
		a && (o.ref ||= a, G(a, t, r), t.seen.get(a).isParent = !0);
	}
	let c = t.metadataRegistry.get(e);
	return c && Kf(o.schema, c), t.io === "input" && K(e) && (delete o.schema.examples, delete o.schema.default), t.io === "input" && "_prefault" in o.schema && ((r = o.schema).default ?? (r.default = o.schema._prefault)), delete o.schema._prefault, t.seen.get(e).schema;
}
function Jf(e) {
	return e.replace(/~/g, "~0").replace(/\//g, "~1");
}
function Yf(e, t) {
	let n = e.seen.get(t);
	if (!n) throw Error("Unprocessed schema. This is a bug in Zod.");
	if (e.external && e.sharedDefsExtractedFor === e.external) return;
	let r = /* @__PURE__ */ new Map();
	for (let t of e.seen.entries()) {
		let n = e.metadataRegistry.get(t[0])?.id;
		if (n) {
			let e = r.get(n);
			if (e && e !== t[0]) throw Error(`Duplicate schema id "${n}" detected during JSON Schema conversion. Two different schemas cannot share the same id when converted together.`);
			r.set(n, t[0]);
		}
	}
	let i = (t) => {
		let r = e.target === "draft-2020-12" ? "$defs" : "definitions";
		if (e.external) {
			let n = e.external.registry.get(t[0])?.id, i = e.external.uri ?? ((e) => e);
			if (n) return { ref: i(n) };
			let a = t[1].defId ?? t[1].schema.id ?? `schema${e.counter++}`;
			return t[1].defId = a, {
				defId: a,
				ref: `${i("__shared")}#/${r}/${Jf(a)}`
			};
		}
		let i = `#/${r}/`;
		if (t[1] === n && !t[1].schema.id) return { ref: "#" };
		let a = t[1].schema.id ?? `__schema${e.counter++}`;
		return {
			defId: a,
			ref: i + Jf(a)
		};
	}, a = (e) => {
		if (e[1].schema.$ref) return;
		let t = e[1], { ref: n, defId: r } = i(e);
		t.def = { ...t.schema }, r && (t.defId = r);
		let a = t.schema;
		for (let e in a) delete a[e];
		a.$ref = n;
	};
	if (e.cycles === "throw") for (let t of e.seen.entries()) {
		let e = t[1];
		if (e.cycle) throw Error(`Cycle detected: #/${e.cycle?.join("/")}/<root>

Set the \`cycles\` parameter to \`"ref"\` to resolve cyclical schemas with defs.`);
	}
	for (let n of e.seen.entries()) {
		let r = n[1];
		if (t === n[0]) a(n);
		else {
			if (e.external) {
				let r = e.external.registry.get(n[0])?.id;
				if (t !== n[0] && r) {
					a(n);
					continue;
				}
			}
			(e.metadataRegistry.get(n[0])?.id || r.cycle || r.count > 1 && e.reused === "ref") && a(n);
		}
	}
	e.external && (e.sharedDefsExtractedFor = e.external);
}
function Xf(e) {
	let t = e.anyOf;
	if (!Array.isArray(t) || t.length === 0 || e.type !== void 0) return;
	let n = [];
	for (let e of t) {
		if (!e || typeof e != "object") return;
		Xf(e);
		let t = Object.keys(e);
		if (t.length !== 1 || t[0] !== "type") return;
		let r = e.type;
		for (let e of Array.isArray(r) ? r : [r]) {
			if (typeof e != "string") return;
			n.includes(e) || n.push(e);
		}
	}
	delete e.anyOf, e.type = n.length === 1 ? n[0] : n;
}
var Zf = /* @__PURE__ */ new Set([
	"type",
	"properties",
	"required",
	"additionalProperties"
]), Qf = ["oneOf", "anyOf"];
function $f(e) {
	let t = e.additionalProperties;
	return t === void 0 || t === !1 || typeof t != "object" || !t ? null : Object.keys(t).length ? t : null;
}
function ep(e) {
	let t = [];
	for (let n of e) {
		if (typeof n != "object" || n.type !== "object") return null;
		for (let e in n) if (!Zf.has(e)) return null;
		t.push(n);
	}
	let n = {}, r = /* @__PURE__ */ new Set();
	for (let e of t) {
		for (let r in e.properties) {
			if (Object.prototype.hasOwnProperty.call(n, r)) continue;
			let e = [];
			for (let n of t) {
				let t = n.properties?.[r] ?? $f(n);
				t != null && (e.some((e) => JSON.stringify(e) === JSON.stringify(t)) || e.push(t));
			}
			_(n, r, e.length === 1 ? e[0] : ep(e) ?? { allOf: e });
		}
		for (let t of e.required ?? []) r.add(t);
	}
	let i = {
		type: "object",
		properties: n
	};
	if (r.size && (i.required = [...r]), t.every((e) => e.additionalProperties === !1)) i.additionalProperties = !1;
	else {
		let e = [];
		for (let n of t) {
			let t = $f(n);
			t && !e.some((e) => JSON.stringify(e) === JSON.stringify(t)) && e.push(t);
		}
		e.length === 1 ? i.additionalProperties = e[0] : e.length > 1 && (i.additionalProperties = { allOf: e });
	}
	return i;
}
function tp(e) {
	let t = e.allOf;
	if (!Array.isArray(t) || t.length < 2) return;
	for (let t of Zf) if (t in e) return;
	let n = t.filter((e) => Qf.some((t) => Array.isArray(e[t]))), r = null;
	if (!n.length) r = ep(t);
	else {
		let e = n[0], i = Qf.find((t) => Array.isArray(e[t]));
		if (Object.keys(e).length !== 1) return;
		let a = t.filter((t) => t !== e), o = e[i].map((e) => ep([...a, e]));
		if (o.some((e) => !e)) return;
		r = { [i]: o };
	}
	r && (delete e.allOf, Kf(e, r));
}
function np(e, t) {
	let n = e.seen.get(t);
	if (!n) throw Error("Unprocessed schema. This is a bug in Zod.");
	let r = (t) => {
		let n = e.seen.get(t);
		if (n.ref === null) return;
		let i = n.def ?? n.schema, a = { ...i }, o = n.ref;
		if (n.ref = null, o) {
			r(o);
			let n = e.seen.get(o), s = n.schema;
			if (s.$ref && (e.target === "draft-07" || e.target === "draft-04" || e.target === "openapi-3.0") ? (i.allOf = i.allOf ?? [], i.allOf.push(s)) : Kf(i, s), Kf(i, a), t._zod.parent === o) for (let e in i) e !== "$ref" && e !== "allOf" && (e in a || delete i[e]);
			if (s.$ref && n.def) for (let e in i) e !== "$ref" && e !== "allOf" && e in n.def && JSON.stringify(i[e]) === JSON.stringify(n.def[e]) && delete i[e];
		}
		let s = t._zod.parent;
		if (s && s !== o) {
			r(s);
			let t = e.seen.get(s);
			if (t?.schema.$ref && (i.$ref = t.schema.$ref, t.def)) for (let e in i) e !== "$ref" && e !== "allOf" && e in t.def && JSON.stringify(i[e]) === JSON.stringify(t.def[e]) && delete i[e];
		}
		e.override({
			zodSchema: t,
			jsonSchema: i,
			path: n.path ?? []
		});
	};
	if (!e.external || e.sharedEmitDoneFor !== e.external) {
		for (let t of [...e.seen.entries()].reverse()) r(t[0]);
		if (e.target !== "openapi-3.0") for (let t of e.seen.entries()) Xf(t[1].def ?? t[1].schema);
		for (let t of e.deferred) t();
		if (e.intersections.length) {
			let t = /* @__PURE__ */ new Map();
			for (let n of e.seen.values()) for (let e of [n.schema, n.def]) {
				let n = e?.allOf;
				if (!Array.isArray(n)) continue;
				let r = t.get(n);
				r ? r.push(e) : t.set(n, [e]);
			}
			for (let n of e.intersections) for (let e of t.get(n) ?? []) tp(e);
		}
	}
	let i = {};
	if (e.target === "draft-2020-12" ? i.$schema = "https://json-schema.org/draft/2020-12/schema" : e.target === "draft-07" ? i.$schema = "http://json-schema.org/draft-07/schema#" : e.target === "draft-04" ? i.$schema = "http://json-schema.org/draft-04/schema#" : e.target, e.external?.uri) {
		let n = e.external.registry.get(t)?.id;
		if (!n) throw Error("Schema is missing an `id` property");
		i.$id = e.external.uri(n);
	}
	Kf(i, n.defId ? n.schema : n.def ?? n.schema);
	let a = e.metadataRegistry.get(t)?.id;
	a !== void 0 && i.id === a && delete i.id;
	let o = e.external?.defs ?? {};
	if (!e.external || e.sharedEmitDoneFor !== e.external) for (let t of e.seen.entries()) {
		let e = t[1];
		e.def && e.defId && (e.def.id === e.defId && delete e.def.id, _(o, e.defId, e.def));
	}
	e.external && (e.sharedEmitDoneFor = e.external), e.external || Object.keys(o).length > 0 && (e.target === "draft-2020-12" ? i.$defs = o : i.definitions = o);
	try {
		let n = JSON.parse(JSON.stringify(i));
		return Object.defineProperty(n, "~standard", {
			value: {
				...t["~standard"],
				jsonSchema: {
					input: ip(t, "input", e.processors),
					output: ip(t, "output", e.processors)
				}
			},
			enumerable: !1,
			writable: !1
		}), n;
	} catch {
		throw Error("Error converting schema to JSON.");
	}
}
function K(e, t) {
	let n = t ?? { seen: /* @__PURE__ */ new Set() };
	if (n.seen.has(e)) return !1;
	n.seen.add(e);
	let r = e._zod.def;
	if (r.type === "transform") return !0;
	if (r.type === "array") return K(r.element, n);
	if (r.type === "set") return K(r.valueType, n);
	if (r.type === "lazy") return K(r.getter(), n);
	if (r.type === "promise" || r.type === "optional" || r.type === "nonoptional" || r.type === "nullable" || r.type === "readonly" || r.type === "default" || r.type === "prefault" || r.type === "catch") return K(r.innerType, n);
	if (r.type === "intersection") return K(r.left, n) || K(r.right, n);
	if (r.type === "record" || r.type === "map") return K(r.keyType, n) || K(r.valueType, n);
	if (r.type === "pipe") return e._zod.traits.has("$ZodCodec") ? !0 : K(r.in, n) || K(r.out, n);
	if (r.type === "object") {
		for (let e in r.shape) if (K(r.shape[e], n)) return !0;
		return !1;
	}
	if (r.type === "union") {
		for (let e of r.options) if (K(e, n)) return !0;
		return !1;
	}
	if (r.type === "tuple") {
		for (let e of r.items) if (K(e, n)) return !0;
		return !!(r.rest && K(r.rest, n));
	}
	return !1;
}
var rp = (e, t = {}) => (n) => {
	let r = qf({
		...n,
		processors: t
	});
	return G(e, r), Yf(r, e), np(r, e);
}, ip = (e, t, n = {}) => (r) => {
	let { libraryOptions: i, target: a } = r ?? {}, o = qf({
		...i ?? {},
		target: a,
		io: t,
		processors: n
	});
	return G(e, o), Yf(o, e), np(o, e);
}, ap = (e, t, n) => {
	(e[t] === void 0 || n > e[t]) && (e[t] = n);
}, op = (e, t, n) => {
	(e[t] === void 0 || n < e[t]) && (e[t] = n);
}, sp = (e, t) => {
	ap(e, "minimum", t), op(e, "maximum", t);
}, cp = (e, t) => {
	e.multipleOf ??= [], e.multipleOf.includes(t) || e.multipleOf.push(t);
}, lp = (e, t) => {
	e.patterns ??= /* @__PURE__ */ new Set(), e.patterns.add(t);
}, up = (e, t) => {
	e.mime = e.mime ? e.mime.filter((e) => t.includes(e)) : [...t];
}, dp = (e, t) => {
	e.format = t, t.includes("int") && (e.isInt = !0);
}, fp = (e, t) => ap(e, "minimum", t.minimum), pp = (e, t) => op(e, "maximum", t.maximum), mp = (e) => (t, n) => {
	dp(t, n.format);
	let [r, i] = e[n.format];
	ap(t, "minimum", r), op(t, "maximum", i);
}, hp = {
	greater_than: (e, t) => ap(e, t.inclusive ? "minimum" : "exclusiveMinimum", t.value),
	less_than: (e, t) => op(e, t.inclusive ? "maximum" : "exclusiveMaximum", t.value),
	multiple_of: (e, t) => cp(e, t.value),
	number_format: mp(Ee),
	bigint_format: mp(De),
	min_length: fp,
	max_length: pp,
	length_equals: (e, t) => sp(e, t.length),
	min_size: fp,
	max_size: pp,
	size_equals: (e, t) => sp(e, t.size),
	string_format: (e, t) => {
		dp(e, t.format), t.pattern && lp(e, t.pattern), (t.format === "base64" || t.format === "base64url") && (e.contentEncoding = t.format), (t.local || t.precision === -1) && (e.laxFormat = !0);
	},
	mime_type: (e, t) => up(e, t.mime)
};
function q(e) {
	let t = {}, n = e._zod.def, r = e._zod.traits.has("$ZodCheck") ? [e, ...n.checks ?? []] : n.checks ?? [];
	for (let e of r) hp[e._zod.def.check]?.(t, e._zod.def);
	let i = e._zod.bag;
	i.minimum !== void 0 && ap(t, "minimum", i.minimum), i.exclusiveMinimum !== void 0 && ap(t, "exclusiveMinimum", i.exclusiveMinimum), i.maximum !== void 0 && op(t, "maximum", i.maximum), i.exclusiveMaximum !== void 0 && op(t, "exclusiveMaximum", i.exclusiveMaximum), i.multipleOf !== void 0 && cp(t, i.multipleOf), i.format !== void 0 && (t.format ??= i.format, i.format.includes("int") && (t.isInt = !0)), i.mime && up(t, i.mime);
	for (let e of i.patterns ?? []) lp(t, e);
	return t;
}
var gp = {
	guid: "uuid",
	url: "uri",
	datetime: "date-time",
	json_string: "json-string",
	regex: ""
}, _p = /* @__PURE__ */ new Map([[Li, zn], [zi, Bn]]), vp = (e) => _p.get(e) ?? e, yp = (e, t, n, r) => {
	let i = n;
	i.type = "string";
	let { minimum: a, maximum: o, format: s, patterns: c, contentEncoding: l, laxFormat: u } = q(e);
	if (typeof a == "number" && (i.minLength = a), typeof o == "number" && (i.maxLength = o), s && (i.format = gp[s] ?? s, i.format === "" && delete i.format, (s === "time" || u) && delete i.format), l && (i.contentEncoding = l), c && c.size > 0) {
		let e = [...c].map(vp);
		e.length === 1 ? i.pattern = e[0].source : e.length > 1 && (i.allOf = [...e.map((e) => ({
			...t.target === "draft-07" || t.target === "draft-04" || t.target === "openapi-3.0" ? { type: "string" } : {},
			pattern: e.source
		}))]);
	}
}, bp = (e, t, n, r) => {
	let i = n, { minimum: a, maximum: o, multipleOf: s, exclusiveMaximum: c, exclusiveMinimum: l, isInt: u } = q(e);
	i.type = u ? "integer" : "number";
	let d = typeof l == "number" && l >= (a ?? -Infinity), f = typeof c == "number" && c <= (o ?? Infinity), p = t.target === "draft-04" || t.target === "openapi-3.0";
	if (d ? p ? (i.minimum = l, i.exclusiveMinimum = !0) : i.exclusiveMinimum = l : typeof a == "number" && (i.minimum = a), f ? p ? (i.maximum = c, i.exclusiveMaximum = !0) : i.exclusiveMaximum = c : typeof o == "number" && (i.maximum = o), s) {
		let n = /* @__PURE__ */ new Set();
		for (let a of s) Number.isFinite(a) && a !== 0 ? n.add(Math.abs(a)) : W(e, t, i, r, `A multipleOf divisor of ${a} cannot be represented in JSON Schema`);
		let [a, ...o] = n;
		a !== void 0 && (i.multipleOf = a), o.length && (i.allOf = [...i.allOf ?? [], ...o.map((e) => ({ multipleOf: e }))]);
	}
}, xp = (e, t, n, r) => {
	n.type = "boolean";
}, Sp = (e, t, n, r) => {
	W(e, t, n, r, "BigInt cannot be represented in JSON Schema");
}, Cp = (e, t, n, r) => {
	W(e, t, n, r, "Symbols cannot be represented in JSON Schema");
}, wp = (e, t, n, r) => {
	t.target === "openapi-3.0" ? (n.type = "string", n.nullable = !0, n.enum = [null]) : n.type = "null";
}, Tp = (e, t, n, r) => {
	W(e, t, n, r, "Undefined cannot be represented in JSON Schema");
}, Ep = (e, t, n, r) => {
	W(e, t, n, r, "Void cannot be represented in JSON Schema");
}, Dp = (e, t, n, r) => {
	n.not = {};
}, Op = (e, t, n, r) => {}, kp = (e, t, n, r) => {}, Ap = (e, t, n, r) => {
	W(e, t, n, r, "Date cannot be represented in JSON Schema");
}, jp = (e, t, n, r) => {
	let i = e._zod.def, a = l(i.entries);
	a.length === 0 ? n.not = {} : (a.every((e) => typeof e == "number") && (n.type = "number"), a.every((e) => typeof e == "string") && (n.type = "string"), n.enum = a);
}, Mp = (e, t, n, r) => {
	let i = e._zod.def;
	if (i.values.length === 0) {
		n.not = {};
		return;
	}
	let a = [];
	for (let o of i.values) if (o === void 0) {
		if (W(e, t, n, r, "Literal `undefined` cannot be represented in JSON Schema")) return;
	} else if (typeof o == "bigint") {
		if (W(e, t, n, r, "BigInt literals cannot be represented in JSON Schema")) return;
		a.push(Number(o));
	} else a.push(o);
	if (a.length !== 0) {
		if (a.length === 1) {
			let e = a[0];
			n.type = e === null ? "null" : typeof e, t.target === "draft-04" || t.target === "openapi-3.0" ? n.enum = [e] : n.const = e;
		} else a.every((e) => typeof e == "number") && (n.type = "number"), a.every((e) => typeof e == "string") && (n.type = "string"), a.every((e) => typeof e == "boolean") && (n.type = "boolean"), a.every((e) => e === null) && (n.type = "null"), n.enum = a;
	}
}, Np = (e, t, n, r) => {
	W(e, t, n, r, "NaN cannot be represented in JSON Schema");
}, Pp = (e, t, n, r) => {
	let i = n, a = e._zod.pattern;
	if (!a) throw Error("Pattern not found in template literal");
	i.type = "string", i.pattern = a.source;
}, Fp = (e, t, n, r) => {
	let i = n;
	i.type = "string", i.format = "binary", i.contentEncoding = "binary";
	let { minimum: a, maximum: o, mime: s } = q(e);
	a !== void 0 && (i.minLength = a), o !== void 0 && (i.maxLength = o), s && (s.length === 0 ? i.not = {} : s.length === 1 ? i.contentMediaType = s[0] : i.anyOf = s.map((e) => ({ contentMediaType: e })));
}, Ip = (e, t, n, r) => {
	n.type = "boolean";
}, Lp = (e, t, n, r) => {
	W(e, t, n, r, "Custom types cannot be represented in JSON Schema");
}, Rp = (e, t, n, r) => {
	W(e, t, n, r, "Function types cannot be represented in JSON Schema");
}, zp = (e, t, n, r) => {
	W(e, t, n, r, "Transforms cannot be represented in JSON Schema");
}, Bp = (e, t, n, r) => {
	W(e, t, n, r, "Map cannot be represented in JSON Schema");
}, Vp = (e, t, n, r) => {
	W(e, t, n, r, "Set cannot be represented in JSON Schema");
}, Hp = (e, t, n, r) => {
	let i = n, a = e._zod.def, { minimum: o, maximum: s } = q(e);
	typeof o == "number" && (i.minItems = o), typeof s == "number" && (i.maxItems = s), i.type = "array", i.items = G(a.element, t, {
		...r,
		path: [...r.path, "items"]
	});
};
function Up(e) {
	let t = e._zod.def;
	return t.type === "pipe" && t.in._zod.traits.has("$ZodTransform") ? Up(t.out) : t.type === "catch" ? Up(t.innerType) : e._zod.optin;
}
var Wp = (e, t, n, r) => {
	let i = n, a = e._zod.def, o = a.shape;
	if (Object.getOwnPropertySymbols(o).length && W(e, t, i, r, "Symbol keys cannot be represented in JSON Schema")) return;
	i.type = "object", i.properties = {};
	for (let e in o) _(i.properties, e, G(o[e], t, {
		...r,
		path: [
			...r.path,
			"properties",
			e
		]
	}));
	let s = [];
	for (let e of Object.keys(o)) {
		let n = a.shape[e];
		(t.io === "input" ? Up(n) === void 0 : n._zod.optout === void 0) && s.push(e);
	}
	s.length > 0 && (i.required = s), a.catchall?._zod.def.type === "never" ? i.additionalProperties = !1 : a.catchall ? a.catchall && (i.additionalProperties = G(a.catchall, t, {
		...r,
		path: [...r.path, "additionalProperties"]
	})) : t.io === "output" && (i.additionalProperties = !1);
}, Gp = (e, t, n, r) => {
	let i = e._zod.def, a = i.inclusive === !1, o = i.options.map((e, n) => G(e, t, {
		...r,
		path: [
			...r.path,
			a ? "oneOf" : "anyOf",
			n
		]
	}));
	a ? n.oneOf = o : n.anyOf = o;
}, Kp = (e, t, n, r) => {
	let i = e._zod.def, a = G(i.left, t, {
		...r,
		path: [
			...r.path,
			"allOf",
			0
		]
	}), o = G(i.right, t, {
		...r,
		path: [
			...r.path,
			"allOf",
			1
		]
	}), s = (e) => "allOf" in e && Object.keys(e).length === 1, c = [...s(a) ? a.allOf : [a], ...s(o) ? o.allOf : [o]];
	n.allOf = c, t.intersections.push(c);
}, qp = (e, t, n, r) => {
	let i = n, a = e._zod.def;
	i.type = "array";
	let o = t.target === "draft-2020-12" ? "prefixItems" : "items", s = t.target === "draft-2020-12" || t.target === "openapi-3.0" ? "items" : "additionalItems", c = a.items.map((e, n) => G(e, t, {
		...r,
		path: [
			...r.path,
			o,
			n
		]
	})), l = a.rest ? G(a.rest, t, {
		...r,
		path: [
			...r.path,
			s,
			...t.target === "openapi-3.0" ? [a.items.length] : []
		]
	}) : null, u = a.items.length;
	for (; u > 0;) {
		let e = a.items[u - 1];
		if (!(t.io === "input" ? Up(e) !== void 0 : e._zod.optout === "optional")) break;
		u--;
	}
	let d = a.items.length, f = !a.rest;
	t.target === "draft-2020-12" ? (i.prefixItems = c, f ? i.items = !1 : l && (i.items = l), u > 0 && (i.minItems = u), f && (i.maxItems = d)) : t.target === "openapi-3.0" ? (i.items = { anyOf: c }, l && i.items.anyOf.push(l), u > 0 && (i.minItems = u), f && (i.maxItems = d)) : (i.items = c, f ? i.additionalItems = !1 : l && (i.additionalItems = l), u > 0 && (i.minItems = u), f && (i.maxItems = d));
	let { minimum: p, maximum: m } = q(e);
	typeof p == "number" && (i.minItems = p), typeof m == "number" && (i.maxItems = m);
};
function Jp(e, t, n) {
	if (t.$ref) {
		if (n.has(t)) return t;
		n.add(t);
		let r = e.get(t)?.def;
		if (!r) return t;
		let i = Jp(e, r, n);
		return i === r ? t : i;
	}
	for (let r of ["anyOf", "oneOf"]) {
		let i = t[r];
		if (!Array.isArray(i)) continue;
		let a = i.map((t) => Jp(e, t, n));
		a.some((e, t) => e !== i[t]) && (t = {
			...t,
			[r]: a
		});
	}
	let r = Array.isArray(t.type) ? t.type : [t.type], i = !r.includes("string") && r.some((e) => e === "number" || e === "integer"), a = t.enum ?? (t.const === void 0 ? void 0 : [t.const]);
	if (!i && !a?.some((e) => typeof e == "number")) return t;
	let { minimum: o, maximum: s, exclusiveMinimum: c, exclusiveMaximum: l, multipleOf: u, format: d, id: f, ...p } = t;
	return p.enum ? p.enum = p.enum.map((e) => typeof e == "number" ? String(e) : e) : typeof p.const == "number" && (p.const = String(p.const)), i && (p.type = "string", a || (p.pattern = (r.includes("number") ? ir : rr).source)), p;
}
var Yp = /* @__PURE__ */ new WeakMap();
function Xp(e) {
	let t = /* @__PURE__ */ new Map();
	for (let n of e.seen.values()) n.def && !t.has(n.schema) && t.set(n.schema, n);
	let n = /* @__PURE__ */ new Map();
	for (let r of Yp.get(e) ?? []) {
		let i = e.seen.get(r), a = (i?.def ?? i?.schema)?.propertyNames;
		if (!a || a === !0 || n.has(a)) continue;
		let o = Jp(t, a, /* @__PURE__ */ new Set());
		o !== a && n.set(a, o);
	}
	if (n.size) for (let t of e.seen.values()) for (let e of [t.schema, t.def]) {
		let t = e && n.get(e.propertyNames);
		t && (e.propertyNames = t);
	}
}
var Zp = (e, t, n, r) => {
	let i = n, a = e._zod.def;
	i.type = "object";
	let o = a.keyType, s = q(o).patterns;
	if (a.mode === "loose" && s && s.size > 0) {
		let e = G(a.valueType, t, {
			...r,
			path: [
				...r.path,
				"patternProperties",
				"*"
			]
		});
		i.patternProperties = {};
		for (let t of s) _(i.patternProperties, vp(t).source, e);
	} else {
		if (t.target === "draft-07" || t.target === "draft-2020-12") {
			i.propertyNames = G(a.keyType, t, {
				...r,
				path: [...r.path, "propertyNames"]
			});
			let n = Yp.get(t);
			n || (n = [], Yp.set(t, n), t.deferred.push(() => Xp(t))), n.push(e);
		}
		i.additionalProperties = G(a.valueType, t, {
			...r,
			path: [...r.path, "additionalProperties"]
		});
	}
	let c = o._zod.values, l = t.io === "input" && Up(a.valueType) !== void 0;
	if (c && !a.partial && !l) {
		let e = [...c].filter((e) => typeof e == "string" || typeof e == "number");
		e.length > 0 && (i.required = e.map(String));
	}
}, Qp = (e, t, n, r) => {
	let i = e._zod.def, a = G(i.innerType, t, r), o = t.seen.get(e);
	t.target === "openapi-3.0" ? (o.ref = i.innerType, n.nullable = !0) : n.anyOf = [a, { type: "null" }];
}, $p = (e, t, n, r) => {
	let i = e._zod.def;
	G(i.innerType, t, r);
	let a = t.seen.get(e);
	a.ref = i.innerType;
}, em = Symbol();
function tm(e, t, n, r, i) {
	let a = !1, o = JSON.stringify(e, (e, t) => typeof t == "bigint" ? (a = !0, null) : t);
	return a ? (W(t, n, r, i, "BigInt defaults cannot be represented in JSON Schema"), em) : JSON.parse(o);
}
var nm = (e, t, n, r) => {
	let i = e._zod.def;
	G(i.innerType, t, r);
	let a = t.seen.get(e);
	a.ref = i.innerType;
	let o = tm(i.defaultValue, e, t, n, r);
	o !== em && (n.default = o);
}, rm = (e, t, n, r) => {
	let i = e._zod.def;
	G(i.innerType, t, r);
	let a = t.seen.get(e);
	if (a.ref = i.innerType, t.io !== "input") return;
	let o = tm(i.defaultValue, e, t, n, r);
	o !== em && (n._prefault = o);
}, im = (e, t, n, r) => {
	let i = e._zod.def;
	G(i.innerType, t, r);
	let a = t.seen.get(e);
	a.ref = i.innerType;
	let o;
	try {
		o = i.catchValue(void 0);
	} catch {
		W(e, t, n, r, "Dynamic catch values are not supported in JSON Schema");
		return;
	}
	n.default = o;
}, am = (e, t, n, r) => {
	let i = e._zod.def, a = i.in._zod.traits.has("$ZodTransform"), o = t.io === "input" ? a ? i.out : i.in : i.out;
	G(o, t, r);
	let s = t.seen.get(e);
	s.ref = o;
}, om = (e, t, n, r) => {
	let i = e._zod.def;
	G(i.innerType, t, r);
	let a = t.seen.get(e);
	a.ref = i.innerType, n.readOnly = !0;
}, sm = (e, t, n, r) => {
	let i = e._zod.def;
	G(i.innerType, t, r);
	let a = t.seen.get(e);
	a.ref = i.innerType;
}, cm = (e, t, n, r) => {
	let i = e._zod.def;
	G(i.innerType, t, r);
	let a = t.seen.get(e);
	a.ref = i.innerType;
}, lm = (e, t, n, r) => {
	let i = e._zod.innerType;
	G(i, t, r);
	let a = t.seen.get(e);
	a.ref = i;
}, um = {
	string: yp,
	number: bp,
	boolean: xp,
	bigint: Sp,
	symbol: Cp,
	null: wp,
	undefined: Tp,
	void: Ep,
	never: Dp,
	any: Op,
	unknown: kp,
	date: Ap,
	enum: jp,
	literal: Mp,
	nan: Np,
	template_literal: Pp,
	file: Fp,
	success: Ip,
	custom: Lp,
	function: Rp,
	transform: zp,
	map: Bp,
	set: Vp,
	array: Hp,
	object: Wp,
	union: Gp,
	intersection: Kp,
	tuple: qp,
	record: Zp,
	nullable: Qp,
	nonoptional: $p,
	default: nm,
	prefault: rm,
	catch: im,
	pipe: am,
	readonly: om,
	promise: sm,
	optional: cm,
	lazy: lm
};
function dm(e, t) {
	if ("_idmap" in e) {
		let n = e, r = qf({
			...t,
			processors: um
		}), i = {};
		for (let e of n._idmap.entries()) {
			let [t, n] = e;
			G(n, r);
		}
		let a = {};
		r.external = {
			registry: n,
			uri: t?.uri,
			defs: i
		};
		for (let e of n._idmap.entries()) {
			let [t, n] = e;
			Yf(r, n), _(a, t, np(r, n));
		}
		return Object.keys(i).length > 0 && (a.__shared = { [r.target === "draft-2020-12" ? "$defs" : "definitions"]: i }), { schemas: a };
	}
	let n = qf({
		...t,
		processors: um
	});
	return G(e, n), Yf(n, e), np(n, e);
}
// node_modules/zod/v4/core/json-schema-generator.js
var fm = class {
	get metadataRegistry() {
		return this.ctx.metadataRegistry;
	}
	get target() {
		return this.ctx.target;
	}
	get unrepresentable() {
		return this.ctx.unrepresentable;
	}
	get override() {
		return this.ctx.override;
	}
	get io() {
		return this.ctx.io;
	}
	get counter() {
		return this.ctx.counter;
	}
	set counter(e) {
		this.ctx.counter = e;
	}
	get seen() {
		return this.ctx.seen;
	}
	constructor(e) {
		let t = e?.target ?? "draft-2020-12";
		t === "draft-4" && (t = "draft-04"), t === "draft-7" && (t = "draft-07"), this.ctx = qf({
			processors: um,
			target: t,
			...e?.metadata && { metadata: e.metadata },
			...e?.unrepresentable && { unrepresentable: e.unrepresentable },
			...e?.override && { override: e.override },
			...e?.io && { io: e.io }
		});
	}
	process(e, t = {
		path: [],
		schemaPath: []
	}) {
		return G(e, this.ctx, t);
	}
	emit(e, t) {
		t && (t.cycles && (this.ctx.cycles = t.cycles), t.reused && (this.ctx.reused = t.reused), t.external && (this.ctx.external = t.external)), this.ctx.sharedDefsExtractedFor = void 0, this.ctx.sharedEmitDoneFor = void 0, Yf(this.ctx, e);
		let { "~standard": n, ...r } = np(this.ctx, e);
		return r;
	}
}, pm = /* @__PURE__ */ t({}), mm = /* @__PURE__ */ t({
	$ZodAny: () => sa,
	$ZodArray: () => _a,
	$ZodAsyncError: () => vt,
	$ZodBase64: () => Ri,
	$ZodBase64URL: () => Vi,
	$ZodBigInt: () => na,
	$ZodBigIntFormat: () => ra,
	$ZodBoolean: () => ta,
	$ZodCIDRv4: () => Ni,
	$ZodCIDRv6: () => Fi,
	$ZodCUID: () => yi,
	$ZodCUID2: () => bi,
	$ZodCatch: () => io,
	$ZodCheck: () => M,
	$ZodCheckBigIntFormat: () => Fr,
	$ZodCheckEndsWith: () => Jr,
	$ZodCheckGreaterThan: () => Mr,
	$ZodCheckIncludes: () => Kr,
	$ZodCheckLengthEquals: () => Vr,
	$ZodCheckLessThan: () => jr,
	$ZodCheckLowerCase: () => Wr,
	$ZodCheckMaxLength: () => zr,
	$ZodCheckMaxSize: () => Ir,
	$ZodCheckMimeType: () => Qr,
	$ZodCheckMinLength: () => Br,
	$ZodCheckMinSize: () => Lr,
	$ZodCheckMultipleOf: () => Nr,
	$ZodCheckNumberFormat: () => Pr,
	$ZodCheckOverwrite: () => $r,
	$ZodCheckProperties: () => Zr,
	$ZodCheckProperty: () => Xr,
	$ZodCheckRegex: () => Ur,
	$ZodCheckSizeEquals: () => Rr,
	$ZodCheckStartsWith: () => qr,
	$ZodCheckStringFormat: () => Hr,
	$ZodCheckUpperCase: () => Gr,
	$ZodCodec: () => co,
	$ZodCreditCard: () => Ki,
	$ZodCustom: () => xo,
	$ZodCustomStringFormat: () => Qi,
	$ZodCyclicError: () => Co,
	$ZodDate: () => da,
	$ZodDefault: () => Za,
	$ZodDiscriminatedUnion: () => Aa,
	$ZodE164: () => Hi,
	$ZodEmail: () => ci,
	$ZodEmoji: () => _i,
	$ZodEncodeError: () => yt,
	$ZodEnum: () => Ua,
	$ZodError: () => Et,
	$ZodExactOptional: () => Ya,
	$ZodFile: () => Ga,
	$ZodFunction: () => vo,
	$ZodGUID: () => oi,
	$ZodIBAN: () => Yi,
	$ZodIPv4: () => Oi,
	$ZodIPv6: () => ji,
	$ZodISODate: () => Ti,
	$ZodISODateTime: () => wi,
	$ZodISODuration: () => Di,
	$ZodISOTime: () => Ei,
	$ZodIntersection: () => ja,
	$ZodJWT: () => Zi,
	$ZodKSUID: () => Ci,
	$ZodLazy: () => bo,
	$ZodLiteral: () => Wa,
	$ZodMAC: () => Mi,
	$ZodMap: () => za,
	$ZodNaN: () => ao,
	$ZodNanoID: () => vi,
	$ZodNever: () => la,
	$ZodNonOptional: () => eo,
	$ZodNull: () => oa,
	$ZodNullable: () => Xa,
	$ZodNumber: () => $i,
	$ZodNumberFormat: () => ea,
	$ZodObject: () => Sa,
	$ZodObjectJIT: () => Ca,
	$ZodOptional: () => Ja,
	$ZodPipe: () => oo,
	$ZodPrefault: () => $a,
	$ZodPreprocess: () => fo,
	$ZodPromise: () => yo,
	$ZodReadonly: () => po,
	$ZodRealError: () => j,
	$ZodRecord: () => Ra,
	$ZodRegistry: () => pl,
	$ZodSet: () => Va,
	$ZodString: () => ai,
	$ZodStringFormat: () => P,
	$ZodSuccess: () => no,
	$ZodSymbol: () => ia,
	$ZodTemplateLiteral: () => _o,
	$ZodTransform: () => Ka,
	$ZodTuple: () => Pa,
	$ZodType: () => N,
	$ZodULID: () => xi,
	$ZodURL: () => gi,
	$ZodUUID: () => si,
	$ZodUndefined: () => aa,
	$ZodUnion: () => Ta,
	$ZodUnknown: () => ca,
	$ZodVoid: () => ua,
	$ZodXID: () => Si,
	$ZodXor: () => Da,
	$brand: () => _t,
	$constructor: () => O,
	$input: () => fl,
	$output: () => dl,
	Doc: () => ei,
	INVALID: () => I,
	JSONSchema: () => pm,
	JSONSchemaGenerator: () => fm,
	NEVER: () => pt,
	TimePrecision: () => ud,
	URL_BAD_FORMAT: () => 1,
	URL_UNPARSEABLE: () => 2,
	ZodCompileAsyncError: () => gl,
	ZodCompileUnsupportedError: () => L,
	_any: () => jd,
	_array: () => pf,
	_base64: () => id,
	_base64url: () => ad,
	_bigint: () => wd,
	_boolean: () => Sd,
	_catch: () => Mf,
	_check: () => Vf,
	_cidrv4: () => nd,
	_cidrv6: () => rd,
	_coercedBigint: () => Td,
	_coercedBoolean: () => Cd,
	_coercedDate: () => Id,
	_coercedNumber: () => gd,
	_coercedString: () => Ru,
	_creditCard: () => sd,
	_cuid: () => Ju,
	_cuid2: () => Yu,
	_custom: () => Rf,
	_date: () => Fd,
	_decode: () => Xt,
	_decodeAsync: () => en,
	_default: () => kf,
	_discriminatedUnion: () => gf,
	_e164: () => od,
	_email: () => zu,
	_emoji: () => Ku,
	_encode: () => Jt,
	_encodeAsync: () => Qt,
	_endsWith: () => nf,
	_enum: () => Sf,
	_file: () => Tf,
	_float32: () => vd,
	_float64: () => yd,
	_gt: () => zd,
	_gte: () => U,
	_guid: () => Bu,
	_iban: () => cd,
	_includes: () => ef,
	_int: () => _d,
	_int32: () => bd,
	_int64: () => Ed,
	_intersection: () => _f,
	_ipv4: () => $u,
	_ipv6: () => ed,
	_isoDate: () => fd,
	_isoDateTime: () => dd,
	_isoDuration: () => md,
	_isoTime: () => pd,
	_jwt: () => ld,
	_ksuid: () => Qu,
	_lazy: () => If,
	_length: () => Xd,
	_literal: () => wf,
	_lowercase: () => Qd,
	_lt: () => Rd,
	_lte: () => H,
	_mac: () => td,
	_map: () => bf,
	_max: () => H,
	_maxLength: () => Jd,
	_maxSize: () => Gd,
	_mime: () => of,
	_min: () => U,
	_minLength: () => Yd,
	_minSize: () => Kd,
	_multipleOf: () => Wd,
	_nan: () => Ld,
	_nanoid: () => qu,
	_nativeEnum: () => Cf,
	_negative: () => Vd,
	_never: () => Nd,
	_nonnegative: () => Ud,
	_nonoptional: () => Af,
	_nonpositive: () => Hd,
	_normalize: () => cf,
	_null: () => Ad,
	_nullable: () => Of,
	_number: () => hd,
	_optional: () => Df,
	_overwrite: () => sf,
	_parse: () => Pt,
	_parseAsync: () => It,
	_pipe: () => Nf,
	_positive: () => Bd,
	_promise: () => Lf,
	_properties: () => af,
	_property: () => rf,
	_readonly: () => Pf,
	_record: () => yf,
	_refine: () => zf,
	_regex: () => Zd,
	_safeDecode: () => an,
	_safeDecodeAsync: () => ln,
	_safeEncode: () => nn,
	_safeEncodeAsync: () => sn,
	_safeParse: () => Rt,
	_safeParseAsync: () => Vt,
	_set: () => xf,
	_size: () => qd,
	_slugify: () => ff,
	_startsWith: () => tf,
	_string: () => Lu,
	_stringFormat: () => Gf,
	_stringbool: () => Wf,
	_success: () => jf,
	_superRefine: () => Bf,
	_symbol: () => Od,
	_templateLiteral: () => Ff,
	_toLowerCase: () => uf,
	_toUpperCase: () => df,
	_transform: () => Ef,
	_trim: () => lf,
	_tuple: () => vf,
	_uint32: () => xd,
	_uint64: () => Dd,
	_ulid: () => Xu,
	_undefined: () => kd,
	_union: () => mf,
	_unknown: () => Md,
	_uppercase: () => $d,
	_url: () => Gu,
	_uuid: () => Vu,
	_uuidv4: () => Hu,
	_uuidv6: () => Uu,
	_uuidv7: () => Wu,
	_void: () => Pd,
	_xid: () => Zu,
	_xor: () => hf,
	base64Charset: () => Li,
	base64urlCharset: () => zi,
	canParseURL: () => li,
	clone: () => b,
	compile: () => vl,
	compileFn: () => xl,
	config: () => A,
	createStandardJSONSchemaMethod: () => ip,
	createToJSONSchemaMethod: () => rp,
	decode: () => Zt,
	decodeAsync: () => tn,
	describe: () => Hf,
	encode: () => Yt,
	encodeAsync: () => $t,
	extractDefs: () => Yf,
	finalize: () => np,
	flattenError: () => Ot,
	formatError: () => kt,
	getDiscriminatedOption: () => Oa,
	globalConfig: () => k,
	globalRegistry: () => F,
	handleUnrepresentable: () => W,
	initializeContext: () => qf,
	isBackEdge: () => Bo,
	isRecursiveSchema: () => Po,
	isValidBase64: () => Ii,
	isValidBase64URL: () => Bi,
	isValidCIDRv6: () => Pi,
	isValidCreditCard: () => Gi,
	isValidIBAN: () => Ji,
	isValidIPv6: () => Ai,
	isValidJWT: () => Xi,
	locales: () => ll,
	memoizer: () => zo,
	mergeValues: () => Ma,
	meta: () => Uf,
	parse: () => Ft,
	parseAsync: () => Lt,
	parseURLObject: () => di,
	prettifyError: () => Mt,
	process: () => G,
	processSchema: () => G,
	regexes: () => dn,
	registry: () => ml,
	safeDecode: () => on,
	safeDecodeAsync: () => un,
	safeEncode: () => rn,
	safeEncodeAsync: () => cn,
	safeParse: () => zt,
	safeParseAsync: () => Ht,
	standardProps: () => ii,
	stripTabAndNewline: () => pi,
	toDotPath: () => jt,
	toJSONSchema: () => dm,
	toZod: () => a,
	treeifyError: () => At,
	urlHostnameOk: () => mi,
	urlProtocolOk: () => hi,
	util: () => n,
	validate: () => Gt,
	validateAsync: () => qt,
	validateURL: () => ui,
	version: () => ti,
	withParser: () => yl
}), hm = /* @__PURE__ */ t({
	endsWith: () => nf,
	gt: () => zd,
	gte: () => U,
	includes: () => ef,
	length: () => Xd,
	lowercase: () => Qd,
	lt: () => Rd,
	lte: () => H,
	maxLength: () => Jd,
	maxSize: () => Gd,
	mime: () => of,
	minLength: () => Yd,
	minSize: () => Kd,
	multipleOf: () => Wd,
	negative: () => Vd,
	nonnegative: () => Ud,
	nonpositive: () => Hd,
	normalize: () => cf,
	overwrite: () => sf,
	positive: () => Bd,
	properties: () => af,
	property: () => rf,
	regex: () => Zd,
	size: () => qd,
	slugify: () => ff,
	startsWith: () => tf,
	toLowerCase: () => uf,
	toUpperCase: () => df,
	trim: () => lf,
	uppercase: () => $d
}), gm = /* @__PURE__ */ new WeakSet([Object.prototype, Error.prototype]);
function _m(e, t, n) {
	Object.defineProperty(e, t, {
		configurable: !0,
		enumerable: !1,
		get() {
			let e = n(this);
			return Object.defineProperty(this, t, {
				value: e,
				configurable: !0,
				writable: !0
			}), e;
		},
		set(e) {
			Object.defineProperty(this, t, {
				value: e,
				configurable: !0,
				writable: !0
			});
		}
	});
}
var vm = (e, t) => {
	Et.init(e, t), e.name = "ZodError";
	let n = Object.getPrototypeOf(e);
	gm.has(n) || (gm.add(n), _m(n, "format", (e) => (t) => kt(e, t)), _m(n, "flatten", (e) => (t) => Ot(e, t)), _m(n, "addIssue", (e) => (t) => {
		e.issues.push(t), e.message = JSON.stringify(e.issues, d, 2);
	}), _m(n, "addIssues", (e) => (t) => {
		e.issues.push(...t), e.message = JSON.stringify(e.issues, d, 2);
	}), Object.defineProperty(n, "isEmpty", {
		configurable: !0,
		enumerable: !1,
		get() {
			return this.issues.length === 0;
		}
	}));
}, ym = /*@__PURE__*/ O("ZodError", vm), J = /*@__PURE__*/ O("ZodError", vm, void 0, { Parent: Error }), bm = /* @__PURE__ */ Pt(J), xm = /* @__PURE__ */ It(J), Sm = /* @__PURE__ */ Rt(J), Cm = /* @__PURE__ */ Vt(J), wm = /* @__PURE__ */ Jt(J), Tm = /* @__PURE__ */ Xt(J), Em = /* @__PURE__ */ Qt(J), Dm = /* @__PURE__ */ en(J), Om = /* @__PURE__ */ nn(J), km = /* @__PURE__ */ an(J), Am = /* @__PURE__ */ sn(J), jm = /* @__PURE__ */ ln(J), Mm = /* @__PURE__ */ t({
	ZodAny: () => cg,
	ZodArray: () => vg,
	ZodBase64: () => Ch,
	ZodBase64URL: () => Th,
	ZodBigInt: () => Zh,
	ZodBigIntFormat: () => $h,
	ZodBoolean: () => Yh,
	ZodCIDRv4: () => yh,
	ZodCIDRv6: () => xh,
	ZodCUID: () => rh,
	ZodCUID2: () => ah,
	ZodCatch: () => p_,
	ZodCodec: () => y_,
	ZodCreditCard: () => kh,
	ZodCustom: () => N_,
	ZodCustomStringFormat: () => Fh,
	ZodDate: () => gg,
	ZodDefault: () => a_,
	ZodDiscriminatedUnion: () => kg,
	ZodE164: () => Dh,
	ZodEmail: () => Hm,
	ZodEmoji: () => $m,
	ZodEnum: () => Ug,
	ZodExactOptional: () => e_,
	ZodFile: () => Jg,
	ZodFunction: () => j_,
	ZodGUID: () => Wm,
	ZodIBAN: () => jh,
	ZodIPv4: () => ph,
	ZodIPv6: () => _h,
	ZodISODate: () => zm,
	ZodISODateTime: () => Rm,
	ZodISODuration: () => Vm,
	ZodISOTime: () => Bm,
	ZodInstanceOf: () => B_,
	ZodIntersection: () => jg,
	ZodJWT: () => Nh,
	ZodKSUID: () => dh,
	ZodLazy: () => D_,
	ZodLiteral: () => Kg,
	ZodMAC: () => hh,
	ZodMap: () => zg,
	ZodNaN: () => h_,
	ZodNanoID: () => th,
	ZodNever: () => fg,
	ZodNonOptional: () => l_,
	ZodNull: () => og,
	ZodNullable: () => n_,
	ZodNumber: () => Vh,
	ZodNumberFormat: () => Uh,
	ZodObject: () => xg,
	ZodOptional: () => Qg,
	ZodPipe: () => __,
	ZodPrefault: () => s_,
	ZodPreprocess: () => S_,
	ZodPromise: () => k_,
	ZodReadonly: () => C_,
	ZodRecord: () => Fg,
	ZodSet: () => Vg,
	ZodString: () => Im,
	ZodStringFormat: () => X,
	ZodSuccess: () => d_,
	ZodSymbol: () => ng,
	ZodTemplateLiteral: () => T_,
	ZodTransform: () => Xg,
	ZodTuple: () => Ng,
	ZodType: () => Y,
	ZodULID: () => sh,
	ZodURL: () => Xm,
	ZodUUID: () => Z,
	ZodUndefined: () => ig,
	ZodUnion: () => Tg,
	ZodUnknown: () => ug,
	ZodVoid: () => mg,
	ZodXID: () => lh,
	ZodXor: () => Dg,
	_ZodString: () => Fm,
	_default: () => o_,
	_function: () => M_,
	any: () => lg,
	array: () => yg,
	base64: () => wh,
	base64url: () => Eh,
	bigint: () => Qh,
	boolean: () => Xh,
	catch: () => m_,
	check: () => P_,
	cidrv4: () => bh,
	cidrv6: () => Sh,
	codec: () => b_,
	creditCard: () => Ah,
	cuid: () => ih,
	cuid2: () => oh,
	currencyCode: () => zh,
	custom: () => F_,
	date: () => _g,
	describe: () => R_,
	discriminatedUnion: () => Ag,
	e164: () => Oh,
	email: () => Um,
	emoji: () => eh,
	enum: () => Wg,
	exactOptional: () => t_,
	file: () => Yg,
	float32: () => Gh,
	float64: () => Kh,
	function: () => M_,
	guid: () => Gm,
	hash: () => Bh,
	hex: () => Rh,
	hostname: () => Lh,
	httpUrl: () => Qm,
	iban: () => Mh,
	instanceof: () => V_,
	int: () => Wh,
	int32: () => qh,
	int64: () => eg,
	intersection: () => Mg,
	invertCodec: () => x_,
	ipv4: () => mh,
	ipv6: () => vh,
	json: () => U_,
	jwt: () => Ph,
	keyof: () => bg,
	ksuid: () => fh,
	lazy: () => O_,
	literal: () => qg,
	looseObject: () => wg,
	looseRecord: () => Rg,
	mac: () => gh,
	map: () => Bg,
	meta: () => z_,
	nan: () => g_,
	nanoid: () => nh,
	nativeEnum: () => Gg,
	never: () => pg,
	nonoptional: () => u_,
	null: () => sg,
	nullable: () => r_,
	nullish: () => i_,
	number: () => Hh,
	object: () => Sg,
	optional: () => $g,
	partialRecord: () => Lg,
	pipe: () => v_,
	prefault: () => c_,
	preprocess: () => W_,
	promise: () => A_,
	readonly: () => w_,
	record: () => Ig,
	refine: () => I_,
	set: () => Hg,
	strictObject: () => Cg,
	string: () => Lm,
	stringFormat: () => Ih,
	stringbool: () => H_,
	success: () => f_,
	superRefine: () => L_,
	symbol: () => rg,
	templateLiteral: () => E_,
	transform: () => Zg,
	tuple: () => Pg,
	uint32: () => Jh,
	uint64: () => tg,
	ulid: () => ch,
	undefined: () => ag,
	union: () => Eg,
	unknown: () => dg,
	url: () => Zm,
	uuid: () => Km,
	uuidv4: () => qm,
	uuidv6: () => Jm,
	uuidv7: () => Ym,
	void: () => hg,
	xid: () => uh,
	xor: () => Og
});
function Nm() {
	k.localeError || A(ds());
}
function Pm() {
	k.memoizer || A({ memoizer: zo() });
}
var Y = /*@__PURE__*/ O("ZodType", (e, t) => (Nm(), N.init(e, t), e.def = t, e.type = t.type, e), {
	check(...e) {
		let t = this.def;
		return this.clone(v(t, { checks: [...t.checks ?? [], ...e.map((e) => typeof e == "function" ? { _zod: {
			check: e,
			def: { check: "custom" },
			onattach: []
		} } : e)] }), { parent: !0 });
	},
	with(...e) {
		return this.check(...e);
	},
	clone(e, t) {
		return b(this, e, t);
	},
	brand() {
		return this;
	},
	register(e, t) {
		return e.add(this, t), this;
	},
	refine(e, t) {
		return this.check(I_(e, t));
	},
	superRefine(e, t) {
		return this.check(L_(e, t));
	},
	overwrite(e) {
		return this.check(/* @__PURE__ */ sf(e));
	},
	optional() {
		return $g(this);
	},
	exactOptional() {
		return t_(this);
	},
	nullable() {
		return r_(this);
	},
	nullish() {
		return $g(r_(this));
	},
	nonoptional(e) {
		return u_(this, e);
	},
	array() {
		return yg(this);
	},
	or(e) {
		return Eg([this, e]);
	},
	and(e) {
		return Mg(this, e);
	},
	transform(e) {
		return v_(this, Zg(e));
	},
	default(e) {
		return o_(this, e);
	},
	prefault(e) {
		return c_(this, e);
	},
	catch(e) {
		return m_(this, e);
	},
	pipe(e) {
		return v_(this, e);
	},
	readonly() {
		return w_(this);
	},
	describe(e) {
		let t = this.clone();
		return F.add(t, { description: e }), t;
	},
	meta(...e) {
		if (e.length === 0) return F.get(this);
		let t = this.clone();
		return F.add(t, e[0]), t;
	},
	isOptional() {
		return this.safeParse(void 0).success;
	},
	isNullable() {
		return this.safeParse(null).success;
	},
	apply(e, ...t) {
		return t.length === 0 ? e(this) : e(this, ...t);
	},
	get "~standard"() {
		return nt(this, "~standard", {
			...ii(this),
			jsonSchema: {
				input: ip(this, "input"),
				output: ip(this, "output")
			}
		});
	},
	set "~standard"(e) {
		tt(this, "~standard", e);
	},
	parse: function e(t, n) {
		return bm(this, t, n, { callee: e });
	},
	parseAsync: async function e(t, n) {
		return await xm(this, t, n, { callee: e });
	},
	safeParse(e, t) {
		return Sm(this, e, t);
	},
	async safeParseAsync(e, t) {
		return Cm(this, e, t);
	},
	get spa() {
		return this?.safeParseAsync;
	},
	set spa(e) {
		tt(this, "spa", e);
	},
	validate(e, t) {
		return Gt(this, e, t);
	},
	validateAsync(e, t) {
		return qt(this, e, t);
	},
	encode: function e(t, n) {
		return wm(this, t, n, { callee: e });
	},
	decode: function e(t, n) {
		return Tm(this, t, n, { callee: e });
	},
	encodeAsync: async function e(t, n) {
		return await Em(this, t, n, { callee: e });
	},
	decodeAsync: async function e(t, n) {
		return await Dm(this, t, n, { callee: e });
	},
	safeEncode(e, t) {
		return Om(this, e, t);
	},
	safeDecode(e, t) {
		return km(this, e, t);
	},
	async safeEncodeAsync(e, t) {
		return Am(this, e, t);
	},
	async safeDecodeAsync(e, t) {
		return jm(this, e, t);
	},
	toJSONSchema(e) {
		return rp(this, {})(e);
	},
	get description() {
		return F.get(this)?.description;
	},
	get _def() {
		return this._zod.def;
	}
}), Fm = /*@__PURE__*/ O("_ZodString", (e, t) => {
	ai.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => yp(e, t, n, r);
}, /*@__PURE__*/ rt({
	format: (e) => q(e).format ?? null,
	minLength: (e) => q(e).minimum ?? null,
	maxLength: (e) => q(e).maximum ?? null
}, {
	regex(...e) {
		return this.check(/* @__PURE__ */ Zd(...e));
	},
	includes(...e) {
		return this.check(/* @__PURE__ */ ef(...e));
	},
	startsWith(...e) {
		return this.check(/* @__PURE__ */ tf(...e));
	},
	endsWith(...e) {
		return this.check(/* @__PURE__ */ nf(...e));
	},
	min(...e) {
		return this.check(/* @__PURE__ */ Yd(...e));
	},
	max(...e) {
		return this.check(/* @__PURE__ */ Jd(...e));
	},
	length(...e) {
		return this.check(/* @__PURE__ */ Xd(...e));
	},
	nonempty(...e) {
		return this.check(/* @__PURE__ */ Yd(1, ...e));
	},
	lowercase(e) {
		return this.check(/* @__PURE__ */ Qd(e));
	},
	uppercase(e) {
		return this.check(/* @__PURE__ */ $d(e));
	},
	trim() {
		return this.check(/* @__PURE__ */ lf());
	},
	normalize(...e) {
		return this.check(/* @__PURE__ */ cf(...e));
	},
	toLowerCase() {
		return this.check(/* @__PURE__ */ uf());
	},
	toUpperCase() {
		return this.check(/* @__PURE__ */ df());
	},
	slugify() {
		return this.check(/* @__PURE__ */ ff());
	}
})), Im = /*@__PURE__*/ O("ZodString", (e, t) => {
	ai.init(e, t), Fm.init(e, t);
}, {
	email(e) {
		return this.check(/* @__PURE__ */ zu(Hm, e));
	},
	url(e) {
		return this.check(/* @__PURE__ */ Gu(Xm, e));
	},
	jwt(e) {
		return this.check(/* @__PURE__ */ ld(Nh, e));
	},
	emoji(e) {
		return this.check(/* @__PURE__ */ Ku($m, e));
	},
	guid(e) {
		return this.check(/* @__PURE__ */ Bu(Wm, e));
	},
	uuid(e) {
		return this.check(/* @__PURE__ */ Vu(Z, e));
	},
	uuidv4(e) {
		return this.check(/* @__PURE__ */ Hu(Z, e));
	},
	uuidv6(e) {
		return this.check(/* @__PURE__ */ Uu(Z, e));
	},
	uuidv7(e) {
		return this.check(/* @__PURE__ */ Wu(Z, e));
	},
	nanoid(e) {
		return this.check(/* @__PURE__ */ qu(th, e));
	},
	cuid(e) {
		return this.check(/* @__PURE__ */ Ju(rh, e));
	},
	cuid2(e) {
		return this.check(/* @__PURE__ */ Yu(ah, e));
	},
	ulid(e) {
		return this.check(/* @__PURE__ */ Xu(sh, e));
	},
	base64(e) {
		return this.check(/* @__PURE__ */ id(Ch, e));
	},
	base64url(e) {
		return this.check(/* @__PURE__ */ ad(Th, e));
	},
	xid(e) {
		return this.check(/* @__PURE__ */ Zu(lh, e));
	},
	ksuid(e) {
		return this.check(/* @__PURE__ */ Qu(dh, e));
	},
	ipv4(e) {
		return this.check(/* @__PURE__ */ $u(ph, e));
	},
	ipv6(e) {
		return this.check(/* @__PURE__ */ ed(_h, e));
	},
	cidrv4(e) {
		return this.check(/* @__PURE__ */ nd(yh, e));
	},
	cidrv6(e) {
		return this.check(/* @__PURE__ */ rd(xh, e));
	},
	e164(e) {
		return this.check(/* @__PURE__ */ od(Dh, e));
	},
	datetime(e) {
		return this.check(/* @__PURE__ */ dd(Rm, e));
	},
	date(e) {
		return this.check(/* @__PURE__ */ fd(zm, e));
	},
	time(e) {
		return this.check(/* @__PURE__ */ pd(Bm, e));
	},
	duration(e) {
		return this.check(/* @__PURE__ */ md(Vm, e));
	}
});
function Lm(e) {
	return /* @__PURE__ */ Lu(Im, e);
}
var X = /*@__PURE__*/ O("ZodStringFormat", (e, t) => {
	P.init(e, t), Fm.init(e, t);
}), Rm = /*@__PURE__*/ O("ZodISODateTime", (e, t) => {
	wi.init(e, t), X.init(e, t);
}), zm = /*@__PURE__*/ O("ZodISODate", (e, t) => {
	Ti.init(e, t), X.init(e, t);
}), Bm = /*@__PURE__*/ O("ZodISOTime", (e, t) => {
	Ei.init(e, t), X.init(e, t);
}), Vm = /*@__PURE__*/ O("ZodISODuration", (e, t) => {
	Di.init(e, t), X.init(e, t);
}), Hm = /*@__PURE__*/ O("ZodEmail", (e, t) => {
	ci.init(e, t), X.init(e, t);
});
function Um(e) {
	return /* @__PURE__ */ zu(Hm, e);
}
var Wm = /*@__PURE__*/ O("ZodGUID", (e, t) => {
	oi.init(e, t), X.init(e, t);
});
function Gm(e) {
	return /* @__PURE__ */ Bu(Wm, e);
}
var Z = /*@__PURE__*/ O("ZodUUID", (e, t) => {
	si.init(e, t), X.init(e, t);
});
function Km(e) {
	return /* @__PURE__ */ Vu(Z, e);
}
function qm(e) {
	return /* @__PURE__ */ Hu(Z, e);
}
function Jm(e) {
	return /* @__PURE__ */ Uu(Z, e);
}
function Ym(e) {
	return /* @__PURE__ */ Wu(Z, e);
}
var Xm = /*@__PURE__*/ O("ZodURL", (e, t) => {
	gi.init(e, t), X.init(e, t);
});
function Zm(e) {
	return /* @__PURE__ */ Gu(Xm, e);
}
function Qm(e) {
	return /* @__PURE__ */ Gu(Xm, {
		protocol: Un,
		hostname: Hn,
		...x(e)
	});
}
var $m = /*@__PURE__*/ O("ZodEmoji", (e, t) => {
	_i.init(e, t), X.init(e, t);
});
function eh(e) {
	return /* @__PURE__ */ Ku($m, e);
}
var th = /*@__PURE__*/ O("ZodNanoID", (e, t) => {
	vi.init(e, t), X.init(e, t);
});
function nh(e) {
	return /* @__PURE__ */ qu(th, e);
}
var rh = /*@__PURE__*/ O("ZodCUID", (e, t) => {
	yi.init(e, t), X.init(e, t);
});
function ih(e) {
	return /* @__PURE__ */ Ju(rh, e);
}
var ah = /*@__PURE__*/ O("ZodCUID2", (e, t) => {
	bi.init(e, t), X.init(e, t);
});
function oh(e) {
	return /* @__PURE__ */ Yu(ah, e);
}
var sh = /*@__PURE__*/ O("ZodULID", (e, t) => {
	xi.init(e, t), X.init(e, t);
});
function ch(e) {
	return /* @__PURE__ */ Xu(sh, e);
}
var lh = /*@__PURE__*/ O("ZodXID", (e, t) => {
	Si.init(e, t), X.init(e, t);
});
function uh(e) {
	return /* @__PURE__ */ Zu(lh, e);
}
var dh = /*@__PURE__*/ O("ZodKSUID", (e, t) => {
	Ci.init(e, t), X.init(e, t);
});
function fh(e) {
	return /* @__PURE__ */ Qu(dh, e);
}
var ph = /*@__PURE__*/ O("ZodIPv4", (e, t) => {
	Oi.init(e, t), X.init(e, t);
});
function mh(e) {
	return /* @__PURE__ */ $u(ph, e);
}
var hh = /*@__PURE__*/ O("ZodMAC", (e, t) => {
	Mi.init(e, t), X.init(e, t);
});
function gh(e) {
	return /* @__PURE__ */ td(hh, e);
}
var _h = /*@__PURE__*/ O("ZodIPv6", (e, t) => {
	ji.init(e, t), X.init(e, t);
});
function vh(e) {
	return /* @__PURE__ */ ed(_h, e);
}
var yh = /*@__PURE__*/ O("ZodCIDRv4", (e, t) => {
	Ni.init(e, t), X.init(e, t);
});
function bh(e) {
	return /* @__PURE__ */ nd(yh, e);
}
var xh = /*@__PURE__*/ O("ZodCIDRv6", (e, t) => {
	Fi.init(e, t), X.init(e, t);
});
function Sh(e) {
	return /* @__PURE__ */ rd(xh, e);
}
var Ch = /*@__PURE__*/ O("ZodBase64", (e, t) => {
	Ri.init(e, t), X.init(e, t);
});
function wh(e) {
	return /* @__PURE__ */ id(Ch, e);
}
var Th = /*@__PURE__*/ O("ZodBase64URL", (e, t) => {
	Vi.init(e, t), X.init(e, t);
});
function Eh(e) {
	return /* @__PURE__ */ ad(Th, e);
}
var Dh = /*@__PURE__*/ O("ZodE164", (e, t) => {
	Hi.init(e, t), X.init(e, t);
});
function Oh(e) {
	return /* @__PURE__ */ od(Dh, e);
}
var kh = /*@__PURE__*/ O("ZodCreditCard", (e, t) => {
	Ki.init(e, t), X.init(e, t);
});
function Ah(e) {
	return /* @__PURE__ */ sd(kh, e);
}
var jh = /*@__PURE__*/ O("ZodIBAN", (e, t) => {
	Yi.init(e, t), X.init(e, t);
});
function Mh(e) {
	return /* @__PURE__ */ cd(jh, e);
}
var Nh = /*@__PURE__*/ O("ZodJWT", (e, t) => {
	Zi.init(e, t), X.init(e, t);
});
function Ph(e) {
	return /* @__PURE__ */ ld(Nh, e);
}
var Fh = /*@__PURE__*/ O("ZodCustomStringFormat", (e, t) => {
	Qi.init(e, t), X.init(e, t);
});
function Ih(e, t, n = {}) {
	return /* @__PURE__ */ Gf(Fh, e, t, n);
}
function Lh(e) {
	return /* @__PURE__ */ Gf(Fh, "hostname", Vn, e);
}
function Rh(e) {
	return /* @__PURE__ */ Gf(Fh, "hex", ur, e);
}
function zh(e) {
	return /* @__PURE__ */ Gf(Fh, "currency_code", Kn, e);
}
function Bh(e, t) {
	let n = `${e}_${t?.enc ?? "hex"}`, r = dn[n];
	if (!r) throw Error(`Unrecognized hash format: ${n}`);
	return /* @__PURE__ */ Gf(Fh, n, r, t);
}
var Vh = /*@__PURE__*/ O("ZodNumber", (e, t) => {
	$i.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => bp(e, t, n, r), e.isFinite = !0;
}, /*@__PURE__*/ rt({
	minValue: (e) => {
		let { minimum: t, exclusiveMinimum: n } = q(e);
		return Math.max(t ?? -Infinity, n ?? -Infinity);
	},
	maxValue: (e) => {
		let { maximum: t, exclusiveMaximum: n } = q(e);
		return Math.min(t ?? Infinity, n ?? Infinity);
	},
	isInt: (e) => {
		let { isInt: t, multipleOf: n } = q(e);
		return !!t || !!n?.some(Number.isSafeInteger);
	},
	format: (e) => q(e).format ?? null
}, {
	gt(e, t) {
		return this.check(/* @__PURE__ */ zd(e, t));
	},
	gte(e, t) {
		return this.check(/* @__PURE__ */ U(e, t));
	},
	min(e, t) {
		return this.check(/* @__PURE__ */ U(e, t));
	},
	lt(e, t) {
		return this.check(/* @__PURE__ */ Rd(e, t));
	},
	lte(e, t) {
		return this.check(/* @__PURE__ */ H(e, t));
	},
	max(e, t) {
		return this.check(/* @__PURE__ */ H(e, t));
	},
	int(e) {
		return this.check(Wh(e));
	},
	safe(e) {
		return this.check(Wh(e));
	},
	positive(e) {
		return this.check(/* @__PURE__ */ zd(0, e));
	},
	nonnegative(e) {
		return this.check(/* @__PURE__ */ U(0, e));
	},
	negative(e) {
		return this.check(/* @__PURE__ */ Rd(0, e));
	},
	nonpositive(e) {
		return this.check(/* @__PURE__ */ H(0, e));
	},
	multipleOf(e, t) {
		return this.check(/* @__PURE__ */ Wd(e, t));
	},
	step(e, t) {
		return this.check(/* @__PURE__ */ Wd(e, t));
	},
	finite() {
		return this;
	}
}));
function Hh(e) {
	return /* @__PURE__ */ hd(Vh, e);
}
var Uh = /*@__PURE__*/ O("ZodNumberFormat", (e, t) => {
	ea.init(e, t), Vh.init(e, t);
});
function Wh(e) {
	return /* @__PURE__ */ _d(Uh, e);
}
function Gh(e) {
	return /* @__PURE__ */ vd(Uh, e);
}
function Kh(e) {
	return /* @__PURE__ */ yd(Uh, e);
}
function qh(e) {
	return /* @__PURE__ */ bd(Uh, e);
}
function Jh(e) {
	return /* @__PURE__ */ xd(Uh, e);
}
var Yh = /*@__PURE__*/ O("ZodBoolean", (e, t) => {
	ta.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => xp(e, t, n, r);
});
function Xh(e) {
	return /* @__PURE__ */ Sd(Yh, e);
}
var Zh = /*@__PURE__*/ O("ZodBigInt", (e, t) => {
	na.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Sp(e, t, n, r);
}, /*@__PURE__*/ rt({
	minValue: (e) => q(e).minimum ?? null,
	maxValue: (e) => q(e).maximum ?? null,
	format: (e) => q(e).format ?? null
}, {
	gte(e, t) {
		return this.check(/* @__PURE__ */ U(e, t));
	},
	min(e, t) {
		return this.check(/* @__PURE__ */ U(e, t));
	},
	gt(e, t) {
		return this.check(/* @__PURE__ */ zd(e, t));
	},
	lt(e, t) {
		return this.check(/* @__PURE__ */ Rd(e, t));
	},
	lte(e, t) {
		return this.check(/* @__PURE__ */ H(e, t));
	},
	max(e, t) {
		return this.check(/* @__PURE__ */ H(e, t));
	},
	positive(e) {
		return this.check(/* @__PURE__ */ zd(BigInt(0), e));
	},
	negative(e) {
		return this.check(/* @__PURE__ */ Rd(BigInt(0), e));
	},
	nonpositive(e) {
		return this.check(/* @__PURE__ */ H(BigInt(0), e));
	},
	nonnegative(e) {
		return this.check(/* @__PURE__ */ U(BigInt(0), e));
	},
	multipleOf(e, t) {
		return this.check(/* @__PURE__ */ Wd(e, t));
	}
}));
function Qh(e) {
	return /* @__PURE__ */ wd(Zh, e);
}
var $h = /*@__PURE__*/ O("ZodBigIntFormat", (e, t) => {
	ra.init(e, t), Zh.init(e, t);
});
function eg(e) {
	return /* @__PURE__ */ Ed($h, e);
}
function tg(e) {
	return /* @__PURE__ */ Dd($h, e);
}
var ng = /*@__PURE__*/ O("ZodSymbol", (e, t) => {
	ia.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Cp(e, t, n, r);
});
function rg(e) {
	return /* @__PURE__ */ Od(ng, e);
}
var ig = /*@__PURE__*/ O("ZodUndefined", (e, t) => {
	aa.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Tp(e, t, n, r);
});
function ag(e) {
	return /* @__PURE__ */ kd(ig, e);
}
var og = /*@__PURE__*/ O("ZodNull", (e, t) => {
	oa.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => wp(e, t, n, r);
});
function sg(e) {
	return /* @__PURE__ */ Ad(og, e);
}
var cg = /*@__PURE__*/ O("ZodAny", (e, t) => {
	sa.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (e, t, n) => void 0;
});
function lg() {
	return /* @__PURE__ */ jd(cg);
}
var ug = /*@__PURE__*/ O("ZodUnknown", (e, t) => {
	ca.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (e, t, n) => void 0;
});
function dg() {
	return /* @__PURE__ */ Md(ug);
}
var fg = /*@__PURE__*/ O("ZodNever", (e, t) => {
	la.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Dp(e, t, n, r);
});
function pg(e) {
	return /* @__PURE__ */ Nd(fg, e);
}
var mg = /*@__PURE__*/ O("ZodVoid", (e, t) => {
	ua.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Ep(e, t, n, r);
});
function hg(e) {
	return /* @__PURE__ */ Pd(mg, e);
}
var gg = /*@__PURE__*/ O("ZodDate", (e, t) => {
	da.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Ap(e, t, n, r), e.min = (t, n) => e.check(/* @__PURE__ */ U(t, n)), e.max = (t, n) => e.check(/* @__PURE__ */ H(t, n));
}, /*@__PURE__*/ rt({
	minDate: (e) => {
		let { minimum: t } = q(e);
		return t ? new Date(t) : null;
	},
	maxDate: (e) => {
		let { maximum: t } = q(e);
		return t ? new Date(t) : null;
	}
}, {}));
function _g(e) {
	return /* @__PURE__ */ Fd(gg, e);
}
var vg = /*@__PURE__*/ O("ZodArray", (e, t) => {
	Pm(), _a.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Hp(e, t, n, r), e.element = t.element;
}, {
	min(e, t) {
		return this.check(/* @__PURE__ */ Yd(e, t));
	},
	nonempty(e) {
		return this.check(/* @__PURE__ */ Yd(1, e));
	},
	max(e, t) {
		return this.check(/* @__PURE__ */ Jd(e, t));
	},
	length(e, t) {
		return this.check(/* @__PURE__ */ Xd(e, t));
	},
	unwrap() {
		return this.element;
	}
});
function yg(e, t) {
	return /* @__PURE__ */ pf(vg, e, t);
}
function bg(e) {
	let t = e._zod.def.shape;
	return Wg(Object.keys(t));
}
var xg = /*@__PURE__*/ O("ZodObject", (e, t) => {
	Pm(), Ca.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Wp(e, t, n, r), lt(e, "shape", (e) => e._zod.def.shape, !1);
}, {
	keyof() {
		return Wg(Object.keys(this._zod.def.shape));
	},
	catchall(e) {
		return this.clone(v(this._zod.def, { catchall: e }));
	},
	passthrough() {
		return this.clone(v(this._zod.def, { catchall: dg() }));
	},
	loose() {
		return this.clone(v(this._zod.def, { catchall: dg() }));
	},
	strict() {
		return this.clone(v(this._zod.def, { catchall: pg() }));
	},
	strip() {
		return this.clone(v(this._zod.def, { catchall: void 0 }));
	},
	extend(e) {
		return je(this, e);
	},
	safeExtend(e) {
		return Ne(this, e);
	},
	merge(e) {
		return Pe(this, e);
	},
	pick(e) {
		return Oe(this, e);
	},
	omit(e) {
		return Ae(this, e);
	},
	partial(...e) {
		return Fe(Qg, this, e[0]);
	},
	exactPartial(...e) {
		return Fe(e_, this, e[0], "exactPartial");
	},
	required(...e) {
		return Ie(l_, this, e[0]);
	}
});
function Sg(e, t) {
	return new xg({
		type: "object",
		shape: e ?? {},
		...x(t)
	});
}
function Cg(e, t) {
	return new xg({
		type: "object",
		shape: e,
		catchall: pg(),
		...x(t)
	});
}
function wg(e, t) {
	return new xg({
		type: "object",
		shape: e,
		catchall: dg(),
		...x(t)
	});
}
var Tg = /*@__PURE__*/ O("ZodUnion", (e, t) => {
	Ta.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Gp(e, t, n, r), e.options = t.options;
});
function Eg(e, t) {
	return new Tg({
		type: "union",
		options: e,
		...x(t)
	});
}
var Dg = /*@__PURE__*/ O("ZodXor", (e, t) => {
	Tg.init(e, t), Da.init(e, t), e._zod.processJSONSchema = (t, n, r) => Gp(e, t, n, r), e.options = t.options;
});
function Og(e, t) {
	return new Dg({
		type: "union",
		options: e,
		inclusive: !1,
		...x(t)
	});
}
var kg = /*@__PURE__*/ O("ZodDiscriminatedUnion", (e, t) => {
	Tg.init(e, t), Aa.init(e, t);
});
function Ag(e, t, n) {
	return new kg({
		type: "union",
		options: t,
		discriminator: e,
		...x(n)
	});
}
var jg = /*@__PURE__*/ O("ZodIntersection", (e, t) => {
	ja.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Kp(e, t, n, r);
});
function Mg(e, t) {
	return new jg({
		type: "intersection",
		left: e,
		right: t
	});
}
var Ng = /*@__PURE__*/ O("ZodTuple", (e, t) => {
	Pm(), Pa.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => qp(e, t, n, r);
}, {
	rest(e) {
		return this.clone({
			...this._zod.def,
			rest: e
		});
	},
	partial() {
		let e = this._zod.def;
		if (e.checks?.length) throw Error(".partial() cannot be used on tuple schemas containing refinements");
		return this.clone({
			...e,
			items: e.items.map((e) => new Qg({
				type: "optional",
				innerType: e
			}))
		});
	}
});
function Pg(e, t, n) {
	let r = t instanceof N;
	return new Ng({
		type: "tuple",
		items: e,
		rest: r ? t : null,
		...x(r ? n : t)
	});
}
var Fg = /*@__PURE__*/ O("ZodRecord", (e, t) => {
	Pm(), Ra.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Zp(e, t, n, r), e.keyType = t.keyType, e.valueType = t.valueType;
});
function Ig(e, t, n) {
	return !t || !t._zod ? new Fg({
		type: "record",
		keyType: Lm(),
		valueType: e,
		...x(t)
	}) : new Fg({
		type: "record",
		keyType: e,
		valueType: t,
		...x(n)
	});
}
function Lg(e, t, n) {
	return new Fg({
		type: "record",
		keyType: e,
		valueType: t,
		...x(n),
		partial: !0
	});
}
function Rg(e, t, n) {
	return new Fg({
		type: "record",
		keyType: e,
		valueType: t,
		mode: "loose",
		...x(n)
	});
}
var zg = /*@__PURE__*/ O("ZodMap", (e, t) => {
	Pm(), za.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Bp(e, t, n, r), e.keyType = t.keyType, e.valueType = t.valueType, e.min = (...t) => e.check(/* @__PURE__ */ Kd(...t)), e.nonempty = (t) => e.check(/* @__PURE__ */ Kd(1, t)), e.max = (...t) => e.check(/* @__PURE__ */ Gd(...t)), e.size = (...t) => e.check(/* @__PURE__ */ qd(...t));
});
function Bg(e, t, n) {
	return new zg({
		type: "map",
		keyType: e,
		valueType: t,
		...x(n)
	});
}
var Vg = /*@__PURE__*/ O("ZodSet", (e, t) => {
	Pm(), Va.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Vp(e, t, n, r), e.min = (...t) => e.check(/* @__PURE__ */ Kd(...t)), e.nonempty = (t) => e.check(/* @__PURE__ */ Kd(1, t)), e.max = (...t) => e.check(/* @__PURE__ */ Gd(...t)), e.size = (...t) => e.check(/* @__PURE__ */ qd(...t));
});
function Hg(e, t) {
	return new Vg({
		type: "set",
		valueType: e,
		...x(t)
	});
}
var Ug = /*@__PURE__*/ O("ZodEnum", (e, t) => {
	Ua.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => jp(e, t, n, r), e.enum = t.entries, e.options = [...e._zod.values];
	let n = new Set(Object.keys(t.entries));
	e.extract = (e, r) => {
		let i = {};
		for (let r of e) if (n.has(r)) i[r] = t.entries[r];
		else throw Error(`Key ${r} not found in enum`);
		return new Ug({
			...t,
			checks: [],
			...x(r),
			entries: i
		});
	}, e.exclude = (e, r) => {
		let i = { ...t.entries };
		for (let t of e) if (n.has(t)) delete i[t];
		else throw Error(`Key ${t} not found in enum`);
		return new Ug({
			...t,
			checks: [],
			...x(r),
			entries: i
		});
	};
});
function Wg(e, t) {
	return new Ug({
		type: "enum",
		entries: Array.isArray(e) ? Object.fromEntries(e.map((e) => [e, e])) : e,
		...x(t)
	});
}
function Gg(e, t) {
	return new Ug({
		type: "enum",
		entries: e,
		...x(t)
	});
}
var Kg = /*@__PURE__*/ O("ZodLiteral", (e, t) => {
	Wa.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Mp(e, t, n, r), e.values = new Set(t.values), Object.defineProperty(e, "value", { get() {
		if (t.values.length > 1) throw Error("This schema contains multiple valid literal values. Use `.values` instead.");
		return t.values[0];
	} });
});
function qg(e, t) {
	return new Kg({
		type: "literal",
		values: Array.isArray(e) ? e : [e],
		...x(t)
	});
}
var Jg = /*@__PURE__*/ O("ZodFile", (e, t) => {
	Ga.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Fp(e, t, n, r), e.min = (t, n) => e.check(/* @__PURE__ */ Kd(t, n)), e.max = (t, n) => e.check(/* @__PURE__ */ Gd(t, n)), e.mime = (t, n) => e.check(/* @__PURE__ */ of(Array.isArray(t) ? t : [t], n));
});
function Yg(e) {
	return /* @__PURE__ */ Tf(Jg, e);
}
var Xg = /*@__PURE__*/ O("ZodTransform", (e, t) => {
	Pm(), Ka.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => zp(e, t, n, r), e._zod.parse = (n, r) => {
		if (r.direction === "backward") throw new yt(e.constructor.name);
		n.addIssue = (r) => {
			if (typeof r == "string") n.issues.push(Ge(r, n.value, t));
			else {
				let t = r;
				t.fatal && (t.continue = !1), t.code ??= "custom", "input" in t || (t.input = n.value), t.inst ??= e, n.issues.push(Ge(t));
			}
		};
		let i = t.transform(n.value, n);
		return i instanceof Promise ? i.then((e) => (n.value = e, n)) : (n.value = i, n);
	};
});
function Zg(e) {
	return new Xg({
		type: "transform",
		transform: e
	});
}
var Qg = /*@__PURE__*/ O("ZodOptional", (e, t) => {
	Ja.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => cm(e, t, n, r), e.unwrap = () => e._zod.def.innerType;
});
function $g(e) {
	return new Qg({
		type: "optional",
		innerType: e
	});
}
var e_ = /*@__PURE__*/ O("ZodExactOptional", (e, t) => {
	Ya.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => cm(e, t, n, r), e.unwrap = () => e._zod.def.innerType;
});
function t_(e) {
	return new e_({
		type: "optional",
		innerType: e
	});
}
var n_ = /*@__PURE__*/ O("ZodNullable", (e, t) => {
	Xa.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Qp(e, t, n, r), e.unwrap = () => e._zod.def.innerType;
});
function r_(e) {
	return new n_({
		type: "nullable",
		innerType: e
	});
}
function i_(e) {
	return $g(r_(e));
}
var a_ = /*@__PURE__*/ O("ZodDefault", (e, t) => {
	Za.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => nm(e, t, n, r), e.unwrap = () => e._zod.def.innerType, e.removeDefault = e.unwrap;
});
function o_(e, t) {
	return new a_({
		type: "default",
		innerType: e,
		get defaultValue() {
			return typeof t == "function" ? t() : ve(t);
		}
	});
}
var s_ = /*@__PURE__*/ O("ZodPrefault", (e, t) => {
	$a.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => rm(e, t, n, r), e.unwrap = () => e._zod.def.innerType;
});
function c_(e, t) {
	return new s_({
		type: "prefault",
		innerType: e,
		get defaultValue() {
			return typeof t == "function" ? t() : ve(t);
		}
	});
}
var l_ = /*@__PURE__*/ O("ZodNonOptional", (e, t) => {
	eo.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => $p(e, t, n, r), e.unwrap = () => e._zod.def.innerType;
});
function u_(e, t) {
	return new l_({
		type: "nonoptional",
		innerType: e,
		...x(t)
	});
}
var d_ = /*@__PURE__*/ O("ZodSuccess", (e, t) => {
	no.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Ip(e, t, n, r), e.unwrap = () => e._zod.def.innerType;
});
function f_(e) {
	return new d_({
		type: "success",
		innerType: e
	});
}
var p_ = /*@__PURE__*/ O("ZodCatch", (e, t) => {
	io.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => im(e, t, n, r), e.unwrap = () => e._zod.def.innerType, e.removeCatch = e.unwrap;
});
function m_(e, t) {
	return new p_({
		type: "catch",
		innerType: e,
		catchValue: typeof t == "function" ? t : dt(t)
	});
}
var h_ = /*@__PURE__*/ O("ZodNaN", (e, t) => {
	ao.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Np(e, t, n, r);
});
function g_(e) {
	return /* @__PURE__ */ Ld(h_, e);
}
var __ = /*@__PURE__*/ O("ZodPipe", (e, t) => {
	oo.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => am(e, t, n, r), e.in = t.in, e.out = t.out;
});
function v_(e, t) {
	return new __({
		type: "pipe",
		in: e,
		out: t
	});
}
var y_ = /*@__PURE__*/ O("ZodCodec", (e, t) => {
	__.init(e, t), co.init(e, t);
});
function b_(e, t, n) {
	return new y_({
		type: "pipe",
		in: e,
		out: t,
		transform: n.decode,
		reverseTransform: n.encode
	});
}
function x_(e) {
	let t = e._zod.def;
	return new y_({
		type: "pipe",
		in: t.out,
		out: t.in,
		transform: t.reverseTransform,
		reverseTransform: t.transform
	});
}
var S_ = /*@__PURE__*/ O("ZodPreprocess", (e, t) => {
	__.init(e, t), fo.init(e, t);
}), C_ = /*@__PURE__*/ O("ZodReadonly", (e, t) => {
	po.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => om(e, t, n, r), e.unwrap = () => e._zod.def.innerType;
});
function w_(e) {
	return new C_({
		type: "readonly",
		innerType: e
	});
}
var T_ = /*@__PURE__*/ O("ZodTemplateLiteral", (e, t) => {
	_o.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Pp(e, t, n, r);
});
function E_(e, t) {
	return new T_({
		type: "template_literal",
		parts: e,
		...x(t)
	});
}
var D_ = /*@__PURE__*/ O("ZodLazy", (e, t) => {
	bo.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => lm(e, t, n, r), e.unwrap = () => e._zod.def.getter();
});
function O_(e) {
	return new D_({
		type: "lazy",
		getter: e
	});
}
var k_ = /*@__PURE__*/ O("ZodPromise", (e, t) => {
	yo.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => sm(e, t, n, r), e.unwrap = () => e._zod.def.innerType;
});
function A_(e) {
	return new k_({
		type: "promise",
		innerType: e
	});
}
var j_ = /*@__PURE__*/ O("ZodFunction", (e, t) => {
	vo.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Rp(e, t, n, r);
});
function M_(e) {
	return new j_({
		type: "function",
		input: Array.isArray(e?.input) ? Pg(e?.input) : e?.input ?? yg(dg()),
		output: e?.output ?? dg()
	});
}
var N_ = /*@__PURE__*/ O("ZodCustom", (e, t) => {
	xo.init(e, t), Y.init(e, t), e._zod.processJSONSchema = (t, n, r) => Lp(e, t, n, r);
});
function P_(e) {
	let t = new M({ check: "custom" });
	return t._zod.check = e, t;
}
function F_(e, t) {
	return /* @__PURE__ */ Rf(N_, e ?? (() => !0), t);
}
function I_(e, t = {}) {
	return /* @__PURE__ */ zf(N_, e, t);
}
function L_(e, t) {
	return /* @__PURE__ */ Bf(e, t);
}
var R_ = Hf, z_ = Uf, B_ = /*@__PURE__*/ O("ZodInstanceOf", (e, t) => {
	N_.init(e, t);
}, { properties(e, t) {
	return this.check(/* @__PURE__ */ af(e, t));
} });
function V_(e, t = {}) {
	let n = new B_({
		type: "custom",
		check: "custom",
		fn: (t) => t instanceof e,
		abort: !0,
		...x(t)
	});
	return n._zod.bag.Class = e, n._zod.check = (t) => {
		t.value instanceof e || t.issues.push({
			code: "invalid_type",
			expected: e.name,
			input: t.value,
			inst: n,
			path: [...n._zod.def.path ?? []]
		});
	}, n;
}
var H_ = (...e) => /* @__PURE__ */ Wf({
	Codec: y_,
	Boolean: Yh,
	String: Im
}, ...e);
function U_(e) {
	let t = O_(() => Eg([
		Lm(e),
		Hh(),
		Xh(),
		sg(),
		yg(t),
		Ig(Lm(), t)
	]));
	return t;
}
function W_(e, t) {
	return new S_({
		type: "pipe",
		in: Zg(e),
		out: t
	});
}
// node_modules/zod/v4/classic/compat.js
var G_ = {
	invalid_type: "invalid_type",
	too_big: "too_big",
	too_small: "too_small",
	invalid_format: "invalid_format",
	not_multiple_of: "not_multiple_of",
	unrecognized_keys: "unrecognized_keys",
	invalid_union: "invalid_union",
	invalid_key: "invalid_key",
	invalid_element: "invalid_element",
	invalid_value: "invalid_value",
	custom: "custom"
};
function K_(e) {
	A({ customError: e });
}
function q_() {
	return A().customError;
}
var J_;
J_ ||= {};
// node_modules/zod/v4/classic/iso.js
var Y_ = /* @__PURE__ */ t({
	ZodISODate: () => zm,
	ZodISODateTime: () => Rm,
	ZodISODuration: () => Vm,
	ZodISOTime: () => Bm,
	date: () => Z_,
	datetime: () => X_,
	duration: () => $_,
	time: () => Q_
});
function X_(e) {
	return /* @__PURE__ */ dd(Rm, e);
}
function Z_(e) {
	return /* @__PURE__ */ fd(zm, e);
}
function Q_(e) {
	return /* @__PURE__ */ pd(Bm, e);
}
function $_(e) {
	return /* @__PURE__ */ md(Vm, e);
}
// node_modules/zod/v4/classic/from-json-schema.js
var Q = {
	...Mm,
	...hm,
	iso: Y_
}, ev = /*@__PURE__*/ new Set(/* @__PURE__ */ "$schema.$ref.$defs.definitions.$id.id.$comment.$anchor.$vocabulary.$dynamicRef.$dynamicAnchor.type.enum.const.anyOf.oneOf.allOf.not.properties.required.additionalProperties.patternProperties.propertyNames.minProperties.maxProperties.items.prefixItems.additionalItems.minItems.maxItems.uniqueItems.contains.minContains.maxContains.minLength.maxLength.pattern.format.minimum.maximum.exclusiveMinimum.exclusiveMaximum.multipleOf.description.default.contentEncoding.contentMediaType.contentSchema.unevaluatedItems.unevaluatedProperties.if.then.else.dependentSchemas.dependentRequired.nullable.readOnly".split("."));
function tv(e, t) {
	let n = e.$schema;
	return n === "https://json-schema.org/draft/2020-12/schema" ? "draft-2020-12" : n === "http://json-schema.org/draft-07/schema#" ? "draft-7" : n === "http://json-schema.org/draft-04/schema#" ? "draft-4" : t ?? "draft-2020-12";
}
function nv(e, t) {
	return e.map((e, n) => n < t ? e : e.optional());
}
function rv(e) {
	return e.replace(/~1/g, "/").replace(/~0/g, "~");
}
function iv(e, t) {
	if (!e.startsWith("#")) throw Error("External $ref is not supported, only local refs (#/...) are allowed");
	let n = e.slice(1).split("/").filter(Boolean);
	if (n.length === 0) return t.rootSchema;
	let r = t.version === "draft-2020-12" ? "$defs" : "definitions";
	if (n[0] === r) {
		let r = n[1] === void 0 ? void 0 : rv(n[1]);
		if (!r || !t.defs[r]) throw Error(`Reference not found: ${e}`);
		return t.defs[r];
	}
	throw Error(`Reference not found: ${e}`);
}
function av(e, t) {
	return Q.transform((e) => e).check((n) => {
		let r = n.value;
		if (typeof r != "object" || !r || Array.isArray(r)) return;
		let i = Object.getOwnPropertyNames(r);
		if (t.minProperties !== void 0 && i.length < t.minProperties && n.issues.push({
			origin: "object",
			code: "too_small",
			minimum: t.minProperties,
			inclusive: !0,
			message: `Too small: expected object to have >=${t.minProperties} properties`,
			input: r,
			inst: e,
			continue: !0
		}), t.maxProperties !== void 0 && i.length > t.maxProperties && n.issues.push({
			origin: "object",
			code: "too_big",
			maximum: t.maxProperties,
			inclusive: !0,
			message: `Too big: expected object to have <=${t.maxProperties} properties`,
			input: r,
			inst: e,
			continue: !0
		}), t.keySchema) for (let e of i) {
			let r = t.keySchema.safeParse(e);
			r.success || n.issues.push({
				code: "invalid_key",
				origin: "record",
				issues: r.error.issues,
				input: e,
				path: [e],
				continue: !0
			});
		}
	}).pipe(e);
}
function ov(e, t) {
	if (e === null) return "z";
	let n = typeof e;
	if (n !== "object") {
		if (n === "number" && Number.isNaN(e)) return null;
		let t = String(e);
		return `${n[0]}${t.length}:${t}`;
	}
	if (t.has(e)) return null;
	t.add(e);
	try {
		if (Array.isArray(e)) {
			let n = [];
			for (let r of e) {
				let e = ov(r, t);
				if (e === null) return null;
				n.push(e);
			}
			return `a${n.length}:[${n.join(",")}]`;
		}
		let n = Object.keys(e).sort(), r = [];
		for (let i of n) {
			let n = ov(e[i], t);
			if (n === null) return null;
			r.push(`${i.length}:${i}=${n}`);
		}
		return `o${r.length}:{${r.join(",")}}`;
	} finally {
		t.delete(e);
	}
}
var sv = /*@__PURE__*/ new Set([
	"items",
	"prefixItems",
	"additionalItems",
	"additionalProperties",
	"contains",
	"propertyNames",
	"not",
	"if",
	"then",
	"else",
	"allOf",
	"anyOf",
	"oneOf",
	"unevaluatedItems",
	"unevaluatedProperties",
	"contentSchema"
]), cv = /*@__PURE__*/ new Set([
	"properties",
	"patternProperties",
	"dependentSchemas",
	"dependencies",
	"$defs",
	"definitions"
]);
function lv(e) {
	return typeof e != "object" || !e ? !1 : Array.isArray(e) ? e.some(lv) : typeof e.$ref == "string" || Object.entries(e).some(([e, t]) => sv.has(e) ? lv(t) : !cv.has(e) || typeof t != "object" || !t ? !1 : Object.values(t).some(lv));
}
function uv(e) {
	return e === 1 ? "element" : "elements";
}
function dv(e, t) {
	return Q.transform((e) => e).check((e) => {
		let n = e.value;
		if (Array.isArray(n)) {
			if (t.uniqueItems === !0) {
				let t = /* @__PURE__ */ new Map();
				for (let r = 0; r < n.length; r++) {
					let i = ov(n[r], /* @__PURE__ */ new Set());
					if (i === null) continue;
					let a = t.get(i);
					a === void 0 ? t.set(i, r) : e.issues.push({
						code: "custom",
						message: `Array items must be unique: element at index ${r} duplicates the one at index ${a}`,
						input: n,
						path: [r],
						continue: !0
					});
				}
			}
			if (t.containsSchema) {
				let r = t.minContains ?? 1, i = t.maxContains === void 0 ? Infinity : t.maxContains + 1, a = 0;
				for (let e of n) if (t.containsSchema.safeParse(e).success && ++a >= i) break;
				a < r && e.issues.push({
					code: "custom",
					message: `Array must contain at least ${r} matching ${uv(r)}; found ${a}`,
					input: n,
					continue: !0
				}), t.maxContains !== void 0 && a > t.maxContains && e.issues.push({
					code: "custom",
					message: `Array must contain at most ${t.maxContains} matching ${uv(t.maxContains)}`,
					input: n,
					continue: !0
				});
			}
		}
	}).pipe(e);
}
function fv(e, t) {
	if (e !== !1) return e === void 0 || e === !0 ? Q.any() : $(e, t);
}
var pv = /^(?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d(?:\.\d+)?(?:Z|[+-](?:[01]\d|2[0-3]):[0-5]\d)$/;
function mv(e, t) {
	if (e.not !== void 0) {
		if (typeof e.not == "object" && Object.keys(e.not).length === 0) return Q.never();
		throw Error("not is not supported in Zod (except { not: {} } for never)");
	}
	if (e.unevaluatedItems !== void 0) throw Error("unevaluatedItems is not supported");
	if (e.unevaluatedProperties !== void 0) throw Error("unevaluatedProperties is not supported");
	if (e.if !== void 0 || e.then !== void 0 || e.else !== void 0) throw Error("Conditional schemas (if/then/else) are not supported");
	if (e.dependentSchemas !== void 0 || e.dependentRequired !== void 0) throw Error("dependentSchemas and dependentRequired are not supported");
	if (e.$ref) {
		let n = e.$ref;
		if (t.refs.has(n)) return t.refs.get(n);
		if (t.processing.has(n)) return Q.lazy(() => {
			if (!t.refs.has(n)) throw Error(`Circular reference not resolved: ${n}`);
			return t.refs.get(n);
		});
		t.processing.add(n);
		let r = $(iv(n, t), t);
		return t.refs.set(n, r), t.processing.delete(n), r;
	}
	if (e.enum !== void 0) {
		let n = e.enum;
		if (t.version === "openapi-3.0" && e.nullable === !0 && n.length === 1 && n[0] === null) return Q.null();
		if (n.length === 0) return Q.never();
		if (n.length === 1) return Q.literal(n[0]);
		if (n.every((e) => typeof e == "string")) return Q.enum(n);
		let r = n.map((e) => Q.literal(e));
		return r.length < 2 ? r[0] : Q.union([
			r[0],
			r[1],
			...r.slice(2)
		]);
	}
	if (e.const !== void 0) return Q.literal(e.const);
	let n = e.type;
	if (Array.isArray(n)) {
		let r = n.map((n) => mv({
			...e,
			type: n
		}, t));
		return r.length === 0 ? Q.never() : r.length === 1 ? r[0] : Q.union(r);
	}
	if (!n) return Q.any();
	let r;
	switch (n) {
		case "string": {
			let t = Q.string();
			if (e.format) {
				let n = e.format;
				n === "email" ? t = t.check(Q.email()) : n === "uri" || n === "uri-reference" ? t = t.check(Q.url()) : n === "uuid" || n === "guid" ? t = t.check(Q.uuid()) : n === "date-time" ? t = t.check(Q.iso.datetime({ offset: !0 })) : n === "date" ? t = t.check(Q.iso.date()) : n === "time" ? t = t.check(Q.regex(pv)) : n === "duration" ? t = t.check(Q.iso.duration()) : n === "hostname" ? t = t.check(Q.hostname()) : n === "ipv4" ? t = t.check(Q.ipv4()) : n === "ipv6" ? t = t.check(Q.ipv6()) : n === "mac" ? t = t.check(Q.mac()) : n === "cidr" ? t = t.check(Q.cidrv4()) : n === "cidr-v6" ? t = t.check(Q.cidrv6()) : n === "base64" ? t = t.check(Q.base64()) : n === "base64url" ? t = t.check(Q.base64url()) : n === "e164" ? t = t.check(Q.e164()) : n === "credit_card" ? t = t.check(Q.creditCard()) : n === "iban" ? t = t.check(Q.iban()) : n === "jwt" ? t = t.check(Q.jwt()) : n === "emoji" ? t = t.check(Q.emoji()) : n === "nanoid" ? t = t.check(Q.nanoid()) : n === "cuid" ? t = t.check(Q.cuid()) : n === "cuid2" ? t = t.check(Q.cuid2()) : n === "ulid" ? t = t.check(Q.ulid()) : n === "xid" ? t = t.check(Q.xid()) : n === "ksuid" && (t = t.check(Q.ksuid()));
			}
			typeof e.minLength == "number" && (t = t.min(e.minLength)), typeof e.maxLength == "number" && (t = t.max(e.maxLength)), e.pattern && (t = t.regex(new RegExp(e.pattern))), r = t;
			break;
		}
		case "number":
		case "integer": {
			let t = n === "integer" ? Q.number().int() : Q.number();
			typeof e.minimum == "number" && e.exclusiveMinimum !== !0 && (t = t.min(e.minimum)), typeof e.maximum == "number" && e.exclusiveMaximum !== !0 && (t = t.max(e.maximum)), typeof e.exclusiveMinimum == "number" ? t = t.gt(e.exclusiveMinimum) : e.exclusiveMinimum === !0 && typeof e.minimum == "number" && (t = t.gt(e.minimum)), typeof e.exclusiveMaximum == "number" ? t = t.lt(e.exclusiveMaximum) : e.exclusiveMaximum === !0 && typeof e.maximum == "number" && (t = t.lt(e.maximum)), typeof e.multipleOf == "number" && (t = t.multipleOf(e.multipleOf)), r = t;
			break;
		}
		case "boolean":
			r = Q.boolean();
			break;
		case "null":
			r = Q.null();
			break;
		case "object": {
			let n = {}, i = e.properties || {}, a = new Set(e.required || []), o = typeof e.additionalProperties == "object" ? $(e.additionalProperties, t) : void 0;
			for (let [e, r] of Object.entries(i)) {
				let i = $(r, t);
				_(n, e, a.has(e) ? i : i.optional());
			}
			if (e.patternProperties) {
				let i = e.patternProperties, a = Object.keys(i), o = [];
				for (let e of a) {
					let n = $(i[e], t), r = Q.string().regex(new RegExp(e));
					o.push(Q.looseRecord(r, n));
				}
				let s = [];
				if (Object.keys(n).length > 0 && s.push(Q.object(n).passthrough()), s.push(...o), s.length === 0) r = Q.object({}).passthrough();
				else if (s.length === 1) r = s[0];
				else {
					let e = Q.intersection(s[0], s[1]);
					for (let t = 2; t < s.length; t++) e = Q.intersection(e, s[t]);
					r = e;
				}
				if (e.additionalProperties === !1) {
					let e = Object.keys(n), t = a.map((e) => new RegExp(e)), i = r;
					r = r.check((n) => {
						if (!_e(n.value)) return;
						let r = [];
						for (let i of Object.keys(n.value)) e.includes(i) || t.some((e) => e.test(i)) || r.push(i);
						r.length && n.issues.push({
							code: "unrecognized_keys",
							keys: r,
							input: n.value,
							inst: i
						});
					});
				}
			} else {
				let t = Q.object(n);
				r = e.additionalProperties === !1 ? t.strict() : o ? t.catchall(o) : t.passthrough();
			}
			let s = e.propertyNames !== void 0 && e.propertyNames !== !0, c = typeof e.minProperties == "number" ? e.minProperties : void 0, l = typeof e.maxProperties == "number" ? e.maxProperties : void 0;
			if (s || c !== void 0 || l !== void 0) {
				let n;
				s && (n = $(typeof e.propertyNames == "object" && e.propertyNames.type === void 0 ? {
					type: "string",
					...e.propertyNames
				} : e.propertyNames, t)), r = av(r, {
					keySchema: n,
					minProperties: c,
					maxProperties: l
				});
			}
			break;
		}
		case "array": {
			let n = e.prefixItems, i = e.items;
			if (n && Array.isArray(n)) {
				let a = typeof e.minItems == "number" ? e.minItems : 0, o = nv(n.map((e) => $(e, t)), a), s = Array.isArray(i) ? void 0 : fv(i, t), c = Q.tuple(o);
				r = s ? c.rest(s) : c, typeof e.minItems == "number" && (r = r.check(Q.minLength(e.minItems))), typeof e.maxItems == "number" && (r = r.check(Q.maxLength(e.maxItems)));
			} else if (Array.isArray(i)) {
				let n = typeof e.minItems == "number" ? e.minItems : 0, a = nv(i.map((e) => $(e, t)), n), o = fv(e.additionalItems, t), s = Q.tuple(a);
				r = o ? s.rest(o) : s, typeof e.minItems == "number" && (r = r.check(Q.minLength(e.minItems))), typeof e.maxItems == "number" && (r = r.check(Q.maxLength(e.maxItems)));
			} else if (i !== void 0) {
				let n = $(i, t), a = Q.array(n);
				typeof e.minItems == "number" && (a = a.min(e.minItems)), typeof e.maxItems == "number" && (a = a.max(e.maxItems)), r = a;
			} else r = Q.array(Q.any());
			(e.uniqueItems === !0 || e.contains !== void 0) && (r = dv(r, {
				uniqueItems: e.uniqueItems === !0,
				containsSchema: e.contains === void 0 ? void 0 : $(e.contains, t),
				minContains: typeof e.minContains == "number" ? e.minContains : void 0,
				maxContains: typeof e.maxContains == "number" ? e.maxContains : void 0
			}));
			break;
		}
		default: throw Error(`Unsupported type: ${n}`);
	}
	return r;
}
function $(e, t) {
	if (typeof e == "boolean") return e ? Q.any() : Q.never();
	let n = mv(e, t), r = e.type || e.enum !== void 0 || e.const !== void 0;
	if (e.anyOf && Array.isArray(e.anyOf)) {
		let i = e.anyOf.map((e) => $(e, t)), a = Q.union(i);
		n = r ? Q.intersection(n, a) : a;
	}
	if (e.oneOf && Array.isArray(e.oneOf)) {
		let i = e.oneOf.map((e) => $(e, t)), a = Q.xor(i);
		n = r ? Q.intersection(n, a) : a;
	}
	if (e.allOf && Array.isArray(e.allOf)) {
		if (e.allOf.length === 0) n = r ? n : Q.any();
		else {
			let i = r ? n : $(e.allOf[0], t), a = +!r;
			for (let n = a; n < e.allOf.length; n++) i = Q.intersection(i, $(e.allOf[n], t));
			n = i;
		}
	}
	e.nullable === !0 && t.version === "openapi-3.0" && (n = Q.nullable(n)), e.readOnly === !0 && (n = Q.readonly(n)), e.default !== void 0 && (n = n.default(e.default));
	let i = {};
	for (let t of [
		"$id",
		"id",
		"$comment",
		"$anchor",
		"$vocabulary",
		"$dynamicRef",
		"$dynamicAnchor"
	]) t in e && (i[t] = e[t]);
	for (let t of [
		"contentEncoding",
		"contentMediaType",
		"contentSchema"
	]) t in e && (i[t] = e[t]);
	if (e.type === "object" && e.$ref === void 0) {
		e.propertyNames !== void 0 && !lv(e.propertyNames) && (i.propertyNames = e.propertyNames);
		for (let t of ["minProperties", "maxProperties"]) e[t] !== void 0 && (i[t] = e[t]);
	}
	if (e.type === "array" && e.$ref === void 0) {
		e.contains !== void 0 && !lv(e.contains) && (i.contains = e.contains);
		for (let t of [
			"uniqueItems",
			"minContains",
			"maxContains"
		]) e[t] !== void 0 && (i[t] = e[t]);
	}
	for (let t of Object.keys(e)) ev.has(t) || _(i, t, e[t]);
	return Object.keys(i).length > 0 && t.registry.add(n, i), e.description && (n = n.describe(e.description)), n;
}
function hv(e, t) {
	if (typeof e == "boolean") return e ? Q.any() : Q.never();
	let n;
	try {
		n = JSON.parse(JSON.stringify(e));
	} catch {
		throw Error("fromJSONSchema input is not valid JSON (possibly cyclic); use $defs/$ref for recursive schemas");
	}
	let r = {
		version: tv(n, t?.defaultTarget),
		defs: n.$defs || n.definitions || {},
		refs: /* @__PURE__ */ new Map(),
		processing: /* @__PURE__ */ new Set(),
		rootSchema: n,
		registry: t?.registry ?? F
	};
	return $(n, r);
}
// node_modules/zod/v4/core/visit.js
var gv = Symbol("z.visit/resolving");
function _v(e, t) {
	let n = typeof t == "function" ? t : (e, n) => {
		let r = t[e._zod.def.type];
		return r ? r(e, n) : e;
	}, r = /* @__PURE__ */ new Map();
	function i(e) {
		let t = r.get(e);
		if (t === gv) return new bo({
			type: "lazy",
			getter: () => r.get(e)
		});
		if (t !== void 0) return t;
		r.set(e, gv);
		let i = a(e), o = n(i, i !== e);
		return r.set(e, o), o;
	}
	function a(e) {
		let t = e._zod.def;
		switch (t.type) {
			case "object": {
				let n = t.shape, r = Object.keys(n), a = !1, o = {};
				for (let e of r) {
					let t = i(n[e]);
					t !== n[e] && (a = !0), o[e] = t;
				}
				let s = t.catchall;
				return t.catchall && (s = i(t.catchall), s !== t.catchall && (a = !0)), a ? b(e, {
					...t,
					shape: o,
					catchall: s
				}) : e;
			}
			case "array": {
				let n = i(t.element);
				return n === t.element ? e : b(e, {
					...t,
					element: n
				});
			}
			case "tuple": {
				let n = t.items, r = !1, a = [];
				for (let e of n) {
					let t = i(e);
					t !== e && (r = !0), a.push(t);
				}
				let o = t.rest;
				return t.rest && (o = i(t.rest), o !== t.rest && (r = !0)), r ? b(e, {
					...t,
					items: a,
					rest: o
				}) : e;
			}
			case "record":
			case "map": {
				let n = i(t.keyType), r = i(t.valueType);
				return n === t.keyType && r === t.valueType ? e : b(e, {
					...t,
					keyType: n,
					valueType: r
				});
			}
			case "set": {
				let n = i(t.valueType);
				return n === t.valueType ? e : b(e, {
					...t,
					valueType: n
				});
			}
			case "union": {
				let n = t.options, r = !1, a = [];
				for (let e of n) {
					let t = i(e);
					t !== e && (r = !0), a.push(t);
				}
				return r ? b(e, {
					...t,
					options: a
				}) : e;
			}
			case "intersection": {
				let n = i(t.left), r = i(t.right);
				return n === t.left && r === t.right ? e : b(e, {
					...t,
					left: n,
					right: r
				});
			}
			case "optional":
			case "nullable":
			case "default":
			case "prefault":
			case "catch":
			case "readonly":
			case "nonoptional":
			case "promise":
			case "success": {
				let n = i(t.innerType);
				return n === t.innerType ? e : b(e, {
					...t,
					innerType: n
				});
			}
			case "pipe": {
				let n = i(t.in), r = i(t.out);
				return n === t.in && r === t.out ? e : b(e, {
					...t,
					in: n,
					out: r
				});
			}
			case "function": {
				let n = i(t.input), r = i(t.output);
				return n === t.input && r === t.output ? e : b(e, {
					...t,
					input: n,
					output: r
				});
			}
			case "lazy": {
				let n = t.getter, { _cachedInner: r, ...a } = t;
				return b(e, {
					...a,
					getter: () => i(n())
				});
			}
			case "template_literal":
			case "string":
			case "number":
			case "int":
			case "boolean":
			case "bigint":
			case "symbol":
			case "undefined":
			case "null":
			case "void":
			case "never":
			case "any":
			case "unknown":
			case "date":
			case "nan":
			case "enum":
			case "literal":
			case "file":
			case "transform":
			case "custom": return e;
			default: return e;
		}
	}
	return i(e);
}
// node_modules/zod/v4/classic/deep-partial.js
function vv(e) {
	return _v(e, {
		object: (e) => e.partial(),
		union: (e) => {
			let t = e._zod.def;
			return t.discriminator === void 0 ? e : Eg(t.options);
		}
	});
}
// node_modules/zod/v4/classic/in-out.js
function yv(e, t) {
	if (!t?.length) return e;
	let n = e._zod.def;
	return b(e, v(n, { checks: [...n.checks ?? [], ...t] }), { parent: !0 });
}
function bv(e) {
	return yv(e.out, e.checks);
}
function xv(e) {
	return e.in._zod.traits.has("$ZodTransform") ? bv(e) : e.in;
}
function Sv(e) {
	return _v(e, {
		pipe: (e) => xv(e._zod.def),
		default: (e, t) => t ? $g(e._zod.def.innerType) : e,
		catch: (e, t) => t ? e._zod.def.innerType : e
	});
}
function Cv(e) {
	return _v(e, {
		pipe: (e) => bv(e._zod.def),
		prefault: (e, t) => t ? e._zod.def.innerType : e
	});
}
// node_modules/zod/v4/classic/coerce.js
var wv = /* @__PURE__ */ t({
	bigint: () => Ov,
	boolean: () => Dv,
	date: () => kv,
	number: () => Ev,
	string: () => Tv
});
function Tv(e) {
	return /* @__PURE__ */ Ru(Im, e);
}
function Ev(e) {
	return /* @__PURE__ */ gd(Vh, e);
}
function Dv(e) {
	return /* @__PURE__ */ Cd(Yh, e);
}
function Ov(e) {
	return /* @__PURE__ */ Td(Zh, e);
}
function kv(e) {
	return /* @__PURE__ */ Id(gg, e);
}
// node_modules/zod/v4/classic/external.js
var Av = /* @__PURE__ */ t({
	$brand: () => _t,
	$input: () => fl,
	$output: () => dl,
	INVALID: () => I,
	NEVER: () => pt,
	TimePrecision: () => ud,
	ZodAny: () => cg,
	ZodArray: () => vg,
	ZodBase64: () => Ch,
	ZodBase64URL: () => Th,
	ZodBigInt: () => Zh,
	ZodBigIntFormat: () => $h,
	ZodBoolean: () => Yh,
	ZodCIDRv4: () => yh,
	ZodCIDRv6: () => xh,
	ZodCUID: () => rh,
	ZodCUID2: () => ah,
	ZodCatch: () => p_,
	ZodCodec: () => y_,
	ZodCompileAsyncError: () => gl,
	ZodCompileUnsupportedError: () => L,
	ZodCreditCard: () => kh,
	ZodCustom: () => N_,
	ZodCustomStringFormat: () => Fh,
	ZodDate: () => gg,
	ZodDefault: () => a_,
	ZodDiscriminatedUnion: () => kg,
	ZodE164: () => Dh,
	ZodEmail: () => Hm,
	ZodEmoji: () => $m,
	ZodEnum: () => Ug,
	ZodError: () => ym,
	ZodExactOptional: () => e_,
	ZodFile: () => Jg,
	ZodFirstPartyTypeKind: () => J_,
	ZodFunction: () => j_,
	ZodGUID: () => Wm,
	ZodIBAN: () => jh,
	ZodIPv4: () => ph,
	ZodIPv6: () => _h,
	ZodISODate: () => zm,
	ZodISODateTime: () => Rm,
	ZodISODuration: () => Vm,
	ZodISOTime: () => Bm,
	ZodInstanceOf: () => B_,
	ZodIntersection: () => jg,
	ZodIssueCode: () => G_,
	ZodJWT: () => Nh,
	ZodKSUID: () => dh,
	ZodLazy: () => D_,
	ZodLiteral: () => Kg,
	ZodMAC: () => hh,
	ZodMap: () => zg,
	ZodNaN: () => h_,
	ZodNanoID: () => th,
	ZodNever: () => fg,
	ZodNonOptional: () => l_,
	ZodNull: () => og,
	ZodNullable: () => n_,
	ZodNumber: () => Vh,
	ZodNumberFormat: () => Uh,
	ZodObject: () => xg,
	ZodOptional: () => Qg,
	ZodPipe: () => __,
	ZodPrefault: () => s_,
	ZodPreprocess: () => S_,
	ZodPromise: () => k_,
	ZodReadonly: () => C_,
	ZodRealError: () => J,
	ZodRecord: () => Fg,
	ZodSet: () => Vg,
	ZodString: () => Im,
	ZodStringFormat: () => X,
	ZodSuccess: () => d_,
	ZodSymbol: () => ng,
	ZodTemplateLiteral: () => T_,
	ZodTransform: () => Xg,
	ZodTuple: () => Ng,
	ZodType: () => Y,
	ZodULID: () => sh,
	ZodURL: () => Xm,
	ZodUUID: () => Z,
	ZodUndefined: () => ig,
	ZodUnion: () => Tg,
	ZodUnknown: () => ug,
	ZodVoid: () => mg,
	ZodXID: () => lh,
	ZodXor: () => Dg,
	_ZodString: () => Fm,
	_default: () => o_,
	_function: () => M_,
	any: () => lg,
	array: () => yg,
	base64: () => wh,
	base64url: () => Eh,
	bigint: () => Qh,
	boolean: () => Xh,
	catch: () => m_,
	check: () => P_,
	cidrv4: () => bh,
	cidrv6: () => Sh,
	clone: () => b,
	codec: () => b_,
	coerce: () => wv,
	compile: () => vl,
	config: () => A,
	core: () => mm,
	creditCard: () => Ah,
	cuid: () => ih,
	cuid2: () => oh,
	currencyCode: () => zh,
	custom: () => F_,
	date: () => _g,
	decode: () => Tm,
	decodeAsync: () => Dm,
	deepPartial: () => vv,
	describe: () => R_,
	discriminatedUnion: () => Ag,
	e164: () => Oh,
	email: () => Um,
	emoji: () => eh,
	encode: () => wm,
	encodeAsync: () => Em,
	endsWith: () => nf,
	enum: () => Wg,
	exactOptional: () => t_,
	file: () => Yg,
	flattenError: () => Ot,
	float32: () => Gh,
	float64: () => Kh,
	formatError: () => kt,
	fromJSONSchema: () => hv,
	function: () => M_,
	getDiscriminatedOption: () => Oa,
	getErrorMap: () => q_,
	globalRegistry: () => F,
	gt: () => zd,
	gte: () => U,
	guid: () => Gm,
	hash: () => Bh,
	hex: () => Rh,
	hostname: () => Lh,
	httpUrl: () => Qm,
	iban: () => Mh,
	includes: () => ef,
	input: () => Sv,
	instanceof: () => V_,
	int: () => Wh,
	int32: () => qh,
	int64: () => eg,
	intersection: () => Mg,
	invertCodec: () => x_,
	ipv4: () => mh,
	ipv6: () => vh,
	iso: () => Y_,
	json: () => U_,
	jwt: () => Ph,
	keyof: () => bg,
	ksuid: () => fh,
	lazy: () => O_,
	length: () => Xd,
	literal: () => qg,
	locales: () => ll,
	looseObject: () => wg,
	looseRecord: () => Rg,
	lowercase: () => Qd,
	lt: () => Rd,
	lte: () => H,
	mac: () => gh,
	map: () => Bg,
	maxLength: () => Jd,
	maxSize: () => Gd,
	memoizer: () => zo,
	meta: () => z_,
	mime: () => of,
	minLength: () => Yd,
	minSize: () => Kd,
	multipleOf: () => Wd,
	nan: () => g_,
	nanoid: () => nh,
	nativeEnum: () => Gg,
	negative: () => Vd,
	never: () => pg,
	nonnegative: () => Ud,
	nonoptional: () => u_,
	nonpositive: () => Hd,
	normalize: () => cf,
	null: () => sg,
	nullable: () => r_,
	nullish: () => i_,
	number: () => Hh,
	object: () => Sg,
	optional: () => $g,
	output: () => Cv,
	overwrite: () => sf,
	parse: () => bm,
	parseAsync: () => xm,
	partialRecord: () => Lg,
	pipe: () => v_,
	positive: () => Bd,
	prefault: () => c_,
	preprocess: () => W_,
	prettifyError: () => Mt,
	promise: () => A_,
	properties: () => af,
	property: () => rf,
	readonly: () => w_,
	record: () => Ig,
	refine: () => I_,
	regex: () => Zd,
	regexes: () => dn,
	registry: () => ml,
	safeDecode: () => km,
	safeDecodeAsync: () => jm,
	safeEncode: () => Om,
	safeEncodeAsync: () => Am,
	safeParse: () => Sm,
	safeParseAsync: () => Cm,
	set: () => Hg,
	setErrorMap: () => K_,
	size: () => qd,
	slugify: () => ff,
	startsWith: () => tf,
	strictObject: () => Cg,
	string: () => Lm,
	stringFormat: () => Ih,
	stringbool: () => H_,
	success: () => f_,
	superRefine: () => L_,
	symbol: () => rg,
	templateLiteral: () => E_,
	toJSONSchema: () => dm,
	toLowerCase: () => uf,
	toUpperCase: () => df,
	toZod: () => a,
	transform: () => Zg,
	treeifyError: () => At,
	trim: () => lf,
	tuple: () => Pg,
	uint32: () => Jh,
	uint64: () => tg,
	ulid: () => ch,
	undefined: () => ag,
	union: () => Eg,
	unknown: () => dg,
	uppercase: () => $d,
	url: () => Zm,
	util: () => n,
	uuid: () => Km,
	uuidv4: () => qm,
	uuidv6: () => Jm,
	uuidv7: () => Ym,
	validate: () => Gt,
	validateAsync: () => qt,
	void: () => hg,
	withParser: () => yl,
	xid: () => uh,
	xor: () => Og
});
export { _t as $brand, fl as $input, dl as $output, I as INVALID, pt as NEVER, ud as TimePrecision, cg as ZodAny, vg as ZodArray, Ch as ZodBase64, Th as ZodBase64URL, Zh as ZodBigInt, $h as ZodBigIntFormat, Yh as ZodBoolean, yh as ZodCIDRv4, xh as ZodCIDRv6, rh as ZodCUID, ah as ZodCUID2, p_ as ZodCatch, y_ as ZodCodec, gl as ZodCompileAsyncError, L as ZodCompileUnsupportedError, kh as ZodCreditCard, N_ as ZodCustom, Fh as ZodCustomStringFormat, gg as ZodDate, a_ as ZodDefault, kg as ZodDiscriminatedUnion, Dh as ZodE164, Hm as ZodEmail, $m as ZodEmoji, Ug as ZodEnum, ym as ZodError, e_ as ZodExactOptional, Jg as ZodFile, J_ as ZodFirstPartyTypeKind, j_ as ZodFunction, Wm as ZodGUID, jh as ZodIBAN, ph as ZodIPv4, _h as ZodIPv6, zm as ZodISODate, Rm as ZodISODateTime, Vm as ZodISODuration, Bm as ZodISOTime, B_ as ZodInstanceOf, jg as ZodIntersection, G_ as ZodIssueCode, Nh as ZodJWT, dh as ZodKSUID, D_ as ZodLazy, Kg as ZodLiteral, hh as ZodMAC, zg as ZodMap, h_ as ZodNaN, th as ZodNanoID, fg as ZodNever, l_ as ZodNonOptional, og as ZodNull, n_ as ZodNullable, Vh as ZodNumber, Uh as ZodNumberFormat, xg as ZodObject, Qg as ZodOptional, __ as ZodPipe, s_ as ZodPrefault, S_ as ZodPreprocess, k_ as ZodPromise, C_ as ZodReadonly, J as ZodRealError, Fg as ZodRecord, Vg as ZodSet, Im as ZodString, X as ZodStringFormat, d_ as ZodSuccess, ng as ZodSymbol, T_ as ZodTemplateLiteral, Xg as ZodTransform, Ng as ZodTuple, Y as ZodType, sh as ZodULID, Xm as ZodURL, Z as ZodUUID, ig as ZodUndefined, Tg as ZodUnion, ug as ZodUnknown, mg as ZodVoid, lh as ZodXID, Dg as ZodXor, Fm as _ZodString, o_ as _default, M_ as _function, M_ as function, lg as any, yg as array, wh as base64, Eh as base64url, Qh as bigint, Xh as boolean, m_ as catch, P_ as check, bh as cidrv4, Sh as cidrv6, b as clone, b_ as codec, wv as coerce, vl as compile, A as config, mm as core, Ah as creditCard, ih as cuid, oh as cuid2, zh as currencyCode, F_ as custom, _g as date, Tm as decode, Dm as decodeAsync, vv as deepPartial, R_ as describe, Ag as discriminatedUnion, Oh as e164, Um as email, eh as emoji, wm as encode, Em as encodeAsync, nf as endsWith, Wg as enum, t_ as exactOptional, Yg as file, Ot as flattenError, Gh as float32, Kh as float64, kt as formatError, hv as fromJSONSchema, Oa as getDiscriminatedOption, q_ as getErrorMap, F as globalRegistry, zd as gt, U as gte, Gm as guid, Bh as hash, Rh as hex, Lh as hostname, Qm as httpUrl, Mh as iban, ef as includes, Sv as input, V_ as instanceof, Wh as int, qh as int32, eg as int64, Mg as intersection, x_ as invertCodec, mh as ipv4, vh as ipv6, Y_ as iso, U_ as json, Ph as jwt, bg as keyof, fh as ksuid, O_ as lazy, Xd as length, qg as literal, ll as locales, wg as looseObject, Rg as looseRecord, Qd as lowercase, Rd as lt, H as lte, gh as mac, Bg as map, Jd as maxLength, Gd as maxSize, zo as memoizer, z_ as meta, of as mime, Yd as minLength, Kd as minSize, Wd as multipleOf, g_ as nan, nh as nanoid, Gg as nativeEnum, Vd as negative, pg as never, Ud as nonnegative, u_ as nonoptional, Hd as nonpositive, cf as normalize, sg as null, r_ as nullable, i_ as nullish, Hh as number, Sg as object, $g as optional, Cv as output, sf as overwrite, bm as parse, xm as parseAsync, Lg as partialRecord, v_ as pipe, Bd as positive, c_ as prefault, W_ as preprocess, Mt as prettifyError, A_ as promise, af as properties, rf as property, w_ as readonly, Ig as record, I_ as refine, Zd as regex, dn as regexes, ml as registry, km as safeDecode, jm as safeDecodeAsync, Om as safeEncode, Am as safeEncodeAsync, Sm as safeParse, Cm as safeParseAsync, Hg as set, K_ as setErrorMap, qd as size, ff as slugify, tf as startsWith, Cg as strictObject, Lm as string, Ih as stringFormat, H_ as stringbool, f_ as success, L_ as superRefine, rg as symbol, E_ as templateLiteral, dm as toJSONSchema, uf as toLowerCase, df as toUpperCase, a as toZod, Zg as transform, At as treeifyError, lf as trim, Pg as tuple, Jh as uint32, tg as uint64, ch as ulid, ag as undefined, Eg as union, dg as unknown, $d as uppercase, Zm as url, n as util, Km as uuid, qm as uuidv4, Jm as uuidv6, Ym as uuidv7, Gt as validate, qt as validateAsync, hg as void, yl as withParser, uh as xid, Og as xor, Av as z };
