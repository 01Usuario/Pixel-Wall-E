using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
public class ValidatorRunner
{
    // 1. Instanciar todos los validadores
    private SpawnValidator spawnValidator;
    private ColorValidator colorValidator = new ColorValidator();
    private SizeValidator sizeValidator;
    private DrawCommandValidator drawCommandValidator = new DrawCommandValidator();
    private FunctionValidator functionValidator = new FunctionValidator();
    private GoToValidator gotoValidator = new GoToValidator();

    public ValidatorRunner(int canvasSize)
    {
        // 2. Inicializar validadores con dependencias
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
                    new FillValidator().Validate(fillNode, context);
                    break;

                default:
                    context.AddError($"Instrucción no soportada: {instruction.GetType().Name}");
                    break;
            }
        }
        

    }
}