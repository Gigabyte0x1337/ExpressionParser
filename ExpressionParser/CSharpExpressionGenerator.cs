using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ExpressionParser
{
    public class CSharpExpressionGenerator
    {
        private Dictionary<string, object> Context { get; set; }

        private Dictionary<string, System.Linq.Expressions.ParameterExpression> Parameters { get; set; }

        public CSharpExpressionGenerator(Dictionary<string, object> context)
        {
            Context = context;

            Parameters = Context.ToDictionary(
                s => s.Key,
                s => System.Linq.Expressions.Expression.Variable(s.Value.GetType(), s.Key)
            );
        }

        public object Invoke(Expression expression)
        {
            var csExpression = Evaluate(expression);

            return System.Linq.Expressions.Expression.Lambda(
                    csExpression,
                    Parameters.Select(o => o.Value)
                )
                .Compile()
                .DynamicInvoke(Context.Select(o => o.Value).ToArray());
        }

        public System.Linq.Expressions.Expression Evaluate(Expression expression)
        {
            switch (expression)
            {
                case TernaryExpression ternaryExpression:
                    return EvaluateTernaryExpression(ternaryExpression);
                    break;
                case BinaryExpression binaryExpression:
                    return EvaluateBinaryExpression(binaryExpression);
                    break;
                case UnaryExpression unaryExpression:
                    return EvaluateUnaryExpression(unaryExpression);
                    break;
                case GroupExpression groupExpression:
                    return EvaluateGroupExpression(groupExpression);
                    break;
                case FunctionCallExpression functionCallExpression:
                    return EvaluateFunctionCallExpression(functionCallExpression);
                    break;
                case ConstantExpression constantExpression:
                    return EvaluateConstantExpression(constantExpression);
                    break;
                case VariableExpression variableExpression:
                    return EvaluateVariableExpression(variableExpression);
                    break;
                case MemberAccessExpression memberAccessExpression:
                    return EvaluateMemberAccessExpression(memberAccessExpression);
                    break;
                case ArrayAccessExpression arrayAccessExpression:
                    return EvaluateArrayAccessExpression(arrayAccessExpression);
                    break;
            }

            return null;
        }

        private System.Linq.Expressions.Expression EvaluateArrayAccessExpression(ArrayAccessExpression arrayAccessExpression)
        {
            return System.Linq.Expressions.Expression.ArrayAccess(Evaluate(arrayAccessExpression.Target), Evaluate(arrayAccessExpression.Index));
        }

        private System.Linq.Expressions.Expression EvaluateMemberAccessExpression(MemberAccessExpression memberAccessExpression)
        {
            var targetExpression = Evaluate(memberAccessExpression.Target);

            switch (memberAccessExpression.Member)
            {
                case FunctionCallExpression functionCallExpression:
                    return System.Linq.Expressions.Expression.Call(
                        targetExpression,
                        targetExpression.Type.GetMethod(functionCallExpression.Name),
                        functionCallExpression.Arguments.Select(Evaluate)
                    );
                    break;
                case VariableExpression variableExpression:
                    return System.Linq.Expressions.Expression.MakeMemberAccess(
                        targetExpression,
                        targetExpression.Type.GetProperty(variableExpression.Name)
                    );
            }

            return null;
        }

        private System.Linq.Expressions.Expression EvaluateVariableExpression(VariableExpression variableExpression)
        {
            return Parameters[variableExpression.Name];
        }

        private System.Linq.Expressions.Expression EvaluateConstantExpression(ConstantExpression constantExpression)
        {
            switch (constantExpression.Type)
            {
                case TokenType.KeywordTrue:
                    return System.Linq.Expressions.Expression.Constant(true);
                case TokenType.KeywordFalse:
                    return System.Linq.Expressions.Expression.Constant(false);
                case TokenType.Decimal:
                    return System.Linq.Expressions.Expression.Constant(decimal.Parse(constantExpression.Value, NumberStyles.Any,CultureInfo.InvariantCulture));
                case TokenType.Int:
                    return System.Linq.Expressions.Expression.Constant(int.Parse(constantExpression.Value));
                case TokenType.StringLiteral:
                    return System.Linq.Expressions.Expression.Constant(constantExpression.Value);
            }

            throw new Exception("Type not found");
        }

        private System.Linq.Expressions.Expression EvaluateFunctionCallExpression(FunctionCallExpression functionCallExpression)
        {
            var arguments = functionCallExpression.Arguments.Select(Evaluate);
            var func = (Delegate)Context[functionCallExpression.Name];

            return System.Linq.Expressions.Expression.Convert(System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.Constant(func),
                typeof(Delegate).GetMethod(nameof(Delegate.DynamicInvoke), new Type[] { typeof(object[]) }),
               System.Linq.Expressions.Expression.NewArrayInit(typeof(object), arguments.Select(o => System.Linq.Expressions.Expression.TypeAs(o, typeof(object))))
             ), func.Method.ReturnType);
        }

        private System.Linq.Expressions.Expression EvaluateGroupExpression(GroupExpression groupExpression)
        {
            return Evaluate(groupExpression.Expression);
        }

        private System.Linq.Expressions.Expression EvaluateUnaryExpression(UnaryExpression unaryExpression)
        {
            switch (unaryExpression.Operator)
            {
                case UnaryOperator.IncrementPrefix:
                    return System.Linq.Expressions.Expression.PreIncrementAssign(Evaluate(unaryExpression.Expression));
                case UnaryOperator.DecrementPrefix:
                    return System.Linq.Expressions.Expression.PreDecrementAssign(Evaluate(unaryExpression.Expression));
                case UnaryOperator.IncrementSuffix:
                    return System.Linq.Expressions.Expression.PostIncrementAssign(Evaluate(unaryExpression.Expression));
                case UnaryOperator.DecrementSuffix:
                    return System.Linq.Expressions.Expression.PostDecrementAssign(Evaluate(unaryExpression.Expression));
                case UnaryOperator.Negate:
                    return System.Linq.Expressions.Expression.Negate(Evaluate(unaryExpression.Expression));
                case UnaryOperator.Not:
                    return System.Linq.Expressions.Expression.Not(Evaluate(unaryExpression.Expression));
                default:
                    return Evaluate(unaryExpression.Expression);
            }
        }

        private System.Linq.Expressions.Expression EvaluateBinaryExpression(BinaryExpression binaryExpression)
        {
            var left = Evaluate(binaryExpression.Left);
            var right = Evaluate(binaryExpression.Right);

            switch (binaryExpression.Operator)
            {
                case BinaryOperator.Add:
                    return System.Linq.Expressions.Expression.Add(left, right);
                    break;
                case BinaryOperator.Subtract:
                    return System.Linq.Expressions.Expression.Subtract(left, right);
                    break;
                case BinaryOperator.Multiply:
                    return System.Linq.Expressions.Expression.Multiply(left, right);
                    break;
                case BinaryOperator.Divide:
                    return System.Linq.Expressions.Expression.Divide(left, right);
                    break;
                case BinaryOperator.Modulo:
                    return System.Linq.Expressions.Expression.Modulo(left, right);
                    break;
                case BinaryOperator.Or:
                    return System.Linq.Expressions.Expression.Or(left, right);
                    break;
                case BinaryOperator.And:
                    return System.Linq.Expressions.Expression.And(left, right);
                    break;
                case BinaryOperator.Equal:
                    return System.Linq.Expressions.Expression.Equal(left, right);
                    break;
                case BinaryOperator.NotEqual:
                    return System.Linq.Expressions.Expression.NotEqual(left, right);
                    break;
                case BinaryOperator.GreaterThanOrEqual:
                    return System.Linq.Expressions.Expression.GreaterThanOrEqual(left, right);
                    break;
                case BinaryOperator.LessThanOrEqual:
                    return System.Linq.Expressions.Expression.LessThanOrEqual(left, right);
                    break;
                case BinaryOperator.GreaterThan:
                    return System.Linq.Expressions.Expression.GreaterThan(left, right);
                    break;
                case BinaryOperator.LessThan:
                    return System.Linq.Expressions.Expression.LessThan(left, right);
                    break;
                case BinaryOperator.Exponent:
                    return System.Linq.Expressions.Expression.Call(null, typeof(Math).GetMethod("Pow"), left, right);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private System.Linq.Expressions.Expression EvaluateTernaryExpression(TernaryExpression ternaryExpression)
        {
            var condition = Evaluate(ternaryExpression.Condition);
            var trueExpression = Evaluate(ternaryExpression.TrueExpression);
            var falseExpression = Evaluate(ternaryExpression.FalseExpression);

            return System.Linq.Expressions.Expression.Condition(condition, trueExpression, falseExpression);
        }
    }
}