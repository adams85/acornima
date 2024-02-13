namespace Acornima;

// Naming based on: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Operator_Precedence#table

public enum Operator
{
    Unknown,

    Assignment,
    NullishCoalescingAssignment,
    LogicalOrAssignment,
    LogicalAndAssignment,
    BitwiseOrAssignment,
    BitwiseXorAssignment,
    BitwiseAndAssignment,
    LeftShiftAssignment,
    RightShiftAssignment,
    UnsignedRightShiftAssignment,
    AdditionAssignment,
    SubtractionAssignment,
    MultiplicationAssignment,
    DivisionAssignment,
    RemainderAssignment,
    ExponentiationAssignment,

    NullishCoalescing,
    LogicalOr,
    LogicalAnd,

    BitwiseOr,
    BitwiseXor,
    BitwiseAnd,
    Equality,
    Inequality,
    StrictEquality,
    StrictInequality,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    In,
    InstanceOf,
    LeftShift,
    RightShift,
    UnsignedRightShift,
    Addition,
    Subtraction,
    Multiplication,
    Division,
    Remainder,
    Exponentiation,

    Increment,
    Decrement,

    LogicalNot,
    BitwiseNot,
    UnaryPlus,
    UnaryNegation,
    TypeOf,
    Void,
    Delete,
}
