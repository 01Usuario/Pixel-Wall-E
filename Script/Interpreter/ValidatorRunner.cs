using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
public class ValidatorRunner
{
    private SpawnValidator spawnValidator;
    private ColorValidator colorValidator = new ColorValidator();
    private SizeValidator sizeValidator;
    private DrawCommandValidator drawCommandValidator = new DrawCommandValidator();
    private FunctionValidator functionValidator = new FunctionValidator();
    private GoToValidator gotoValidator = new GoToValidator();
    private FillValidator fillValidator = new FillValidator();
    private LabelValidator labelValidator = new LabelValidator();
    private VariableValidator variableValidator = new VariableValidator();
    private AssignValidator assignValidator = new AssignValidator();

    public ValidatorRunner(int canvasSize)
    {
        spawnValidator = new SpawnValidator(canvasSize);
        sizeValidator = new SizeValidator(canvasSize);
    }

    public void Validate(ProgramNode program, SemanticContext context)
    {
        spawnValidator.Validate(program, context);
        foreach (var instruction in program.Instructions)
        {
            switch (instruction)
            {
                case SpawnNode spawnNode:
                    spawnValidator.Validate(spawnNode, context);
                    break;

                case ColorNode colorNode:
                    colorValidator.Validate(colorNode, context);
                    break;

                case SizeNode sizeNode:
                    sizeValidator.Validate(sizeNode, context);
                    break;

                case DrawCommandNode drawCmd:
                    drawCommandValidator.Validate(drawCmd, context);
                    break;

                case FunctionNode functionNode:
                    functionValidator.Validate(functionNode, context);
                    break;

                case GoToNode gotoNode:
                    gotoValidator.Validate(gotoNode, context);
                    break;

                case FillNode fillNode:
                    fillValidator.Validate(fillNode, context);
                    break;
                case LabelNode labelNode:
                    labelValidator.Validate(labelNode, context);
                    break;
                case VariableNode variableNodeNode:
                   variableValidator.Validate(variableNodeNode, context);
                    break;
                case AssignNode assignNode:
                    assignValidator.Validate(assignNode, context);
                    break;
                default:
                    context.AddError($"Instrucción no soportada: {instruction.GetType().Name}");
                    break;
            }
        }
    }
}