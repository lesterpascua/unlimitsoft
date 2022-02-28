grammar ArithmeticExpression;

/*
 * Parser Rules
 */

compileUnit
	:	expression EOF
	;

expression
	:	term																													#termExpression
	|	LPARENT expression RPARENT																								#parenExpression
	|	FUNCTION LPARENT ((expression (COMMA expression)*)?) RPARENT																#functionExpression
	|	op=(PLUS | MINUS) expression																							#unaryExpression
	|	left=expression op=(TIMES | DIV | MOD) rigth=expression																	#multExpression
	|	left=expression op=(PLUS | MINUS) rigth=expression																		#sumExpression
	|	left=expression op=(AND | OR | EQUAL | NOTEQUAL | LESS | GRATHER | LESSEQUAL | GRATHEREQUAL) rigth=expression			#logicalExpression
	;
term
	: NUMBER | LITERAL
	;

/*
 * Lexer Rules
 */
NUMBER			: ('0'..'9')+ ('.' ('0'..'9')+)?;
FUNCTION		: ('a'..'z'|'A'..'Z')+ ('.' ('a'..'z'|'A'..'Z'|('0'..'9'))+)*;
LITERAL			: '"'('a'..'z'|'A'..'Z'|'0'..'9'|'.')+'"';

LPARENT			: '(';
RPARENT			: ')';

PLUS			: '+';
MINUS			: '-';

TIMES			: '*';
DIV				: '/';
MOD				: '%';

OR				: '||';
AND				: '&&';
EQUAL			: '==';
NOTEQUAL		: '!=';
LESS			: '<';
LESSEQUAL		: '<=';
GRATHER			: '>';
GRATHEREQUAL	: '>=';

COMMA			: ',';

WS				: ' ' -> channel(HIDDEN); // [ \r\n\t] + -> skip;