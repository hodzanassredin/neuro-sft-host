MODULE ObxHello0;

	IMPORT StdLog;

	PROCEDURE Do*;
	BEGIN
		StdLog.String("Hello World"); StdLog.Ln  (* вывод в рабочий журнал (Log) цепочки литер и 0DX (переход на новую строку) *)
	END Do;

END ObxHello0.