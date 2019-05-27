using ExpressionParser.Expression;

namespace ExpressionParser
{
    public class TreeVisitor
    {
        protected void Visit(Expression.Expression expression)
        {
            switch (expression)
            {
                case TernaryExpression ternaryExpression:
                    VisitTernaryExpression(ternaryExpression);
                    break;
                case BinaryExpression binaryExpression:
                    VisitBinaryExpression(binaryExpression);
                    break;
                case UnaryExpression unaryExpression:
                    VisitUnaryExpression(unaryExpression);
                    break;
                case GroupExpression groupExpression:
                    VisitGroupExpression(groupExpression);
                    break;
                case FunctionCallExpression functionCallExpression:
                    VisitFunctionCallExpression(functionCallExpression);
                    break;
                case ConstantExpression constantExpression:
                    VisitConstantExpression(constantExpression);
                    break;
                case VariableExpression variableExpression:
                    VisitVariableExpression(variableExpression);
                    break;
                case MemberAccessExpression memberAccessExpression:
                    VisitMemberAccessExpression(memberAccessExpression);
                    break;
                case ArrayAccessExpression arrayAccessExpression:
                    VisitArrayAccessExpression(arrayAccessExpression);
                    break;
            }
        }

        protected virtual void VisitTernaryExpression(TernaryExpression expression)
        {
            Visit(expression.Condition);
            Visit(expression.TrueExpression);
            Visit(expression.FalseExpression);
        }

        protected virtual void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            Visit(binaryExpression.Left);
            Visit(binaryExpression.Right);
        }

        protected virtual void VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            Visit(unaryExpression.Expression);
        }

        protected virtual void VisitGroupExpression(GroupExpression groupExpression)
        {
            Visit(groupExpression.Expression);
        }

        protected virtual void VisitFunctionCallExpression(FunctionCallExpression functionCallExpression)
        {
            functionCallExpression.Arguments.ForEach(Visit);
        }

        protected virtual void VisitMemberAccessExpression(MemberAccessExpression memberAccessExpression)
        {
            Visit(memberAccessExpression.Target);
            Visit(memberAccessExpression.Member);
        }

        protected virtual void VisitArrayAccessExpression(ArrayAccessExpression arrayAccessExpression)
        {
            Visit(arrayAccessExpression.Target);
            Visit(arrayAccessExpression.Index);
        }

        protected virtual void VisitConstantExpression(ConstantExpression constantExpression)
        {

        }

        protected virtual void VisitVariableExpression(VariableExpression variableExpression)
        {

        }
    }
}